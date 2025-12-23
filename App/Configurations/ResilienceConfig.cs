using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace App.Configurations;

/// <summary>
/// Resilience configuration using Polly for retry, circuit breaker, and timeout policies.
/// Applied to external HTTP calls (Payment APIs, OAuth providers, etc.)
/// </summary>
public static class ResilienceConfig
{
    /// <summary>
    /// Configure resilience policies for HTTP clients.
    /// </summary>
    public static IServiceCollection ConfigureResilience(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure named HTTP clients with resilience policies
        ConfigurePaymentClients(services);
        ConfigureExternalAuthClients(services);
        
        return services;
    }
    
    private static void ConfigurePaymentClients(IServiceCollection services)
    {
        // Stripe API Client with resilience
        services.AddHttpClient("StripeApi", client =>
        {
            client.BaseAddress = new Uri("https://api.stripe.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddStandardResilienceHandler(options =>
        {
            // Retry policy: 3 retries with exponential backoff
            options.Retry = new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = static args => ValueTask.FromResult(ShouldRetry(args.Outcome))
            };
            
            // Circuit breaker: Open after 5 failures, stay open for 30 seconds
            options.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5, // 50% failure rate
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = static args => ValueTask.FromResult(ShouldBreak(args.Outcome))
            };
            
            // Timeout: 10 seconds per request
            options.AttemptTimeout = new HttpTimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            
            // Total timeout: 60 seconds including retries
            options.TotalRequestTimeout = new HttpTimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
        });
        
        // VNPay API Client
        services.AddHttpClient("VNPayApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry = new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(300),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            };
            
            options.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(20)
            };
            
            options.AttemptTimeout = new HttpTimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        });
        
        // MoMo API Client
        services.AddHttpClient("MoMoApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry = new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(300),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            };
            
            options.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(20)
            };
        });
    }
    
    private static void ConfigureExternalAuthClients(IServiceCollection services)
    {
        // Google OAuth API Client (for token validation)
        services.AddHttpClient("GoogleAuth", client =>
        {
            client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry = new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            };
            
            options.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(60),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30)
            };
        });
        
        // Facebook Graph API Client
        services.AddHttpClient("FacebookAuth", client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry = new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential
            };
        });
    }
    
    /// <summary>
    /// Determine if a response should trigger a retry.
    /// </summary>
    private static bool ShouldRetry(Outcome<HttpResponseMessage> outcome)
    {
        // Retry on transient errors
        if (outcome.Exception != null)
        {
            return outcome.Exception is HttpRequestException 
                or TimeoutRejectedException 
                or TaskCanceledException;
        }
        
        if (outcome.Result == null) return true;
        
        // Retry on 5xx errors and specific 4xx
        var statusCode = (int)outcome.Result.StatusCode;
        return statusCode >= 500 || statusCode == 408 || statusCode == 429;
    }
    
    /// <summary>
    /// Determine if a response should trigger circuit breaker.
    /// </summary>
    private static bool ShouldBreak(Outcome<HttpResponseMessage> outcome)
    {
        // Break circuit on server errors
        if (outcome.Exception != null) return true;
        if (outcome.Result == null) return true;
        
        return (int)outcome.Result.StatusCode >= 500;
    }
}
