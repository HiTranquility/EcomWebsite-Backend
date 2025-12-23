-- ============================================
-- Migration: Add Transaction Extended Fields
-- Author: Auto-generated
-- Date: 2024-12-19
-- Description: Adds new fields to transactions table for better payment tracking
-- ============================================

-- Step 1: Add new columns to transactions table
ALTER TABLE `transactions`
    ADD COLUMN `provider_transaction_id` VARCHAR(255) NULL COMMENT 'Transaction ID from payment provider (Stripe PaymentIntent ID, MoMo TransId, VNPay TransactionNo)' AFTER `status`,
    ADD COLUMN `provider_refund_id` VARCHAR(255) NULL COMMENT 'Refund ID from payment provider (if refunded)' AFTER `provider_transaction_id`,
    ADD COLUMN `currency` VARCHAR(10) NOT NULL DEFAULT 'VND' COMMENT 'Currency code (VND, USD, etc.)' AFTER `provider_refund_id`,
    ADD COLUMN `failure_reason` VARCHAR(500) NULL COMMENT 'Reason for payment failure (if failed)' AFTER `currency`,
    ADD COLUMN `payer_info` JSON NULL COMMENT 'JSON containing payer information (email, phone, card last 4 digits, etc.)' AFTER `failure_reason`,
    ADD COLUMN `refund_amount` DECIMAL(10,2) NULL COMMENT 'Amount refunded (for partial refunds)' AFTER `payer_info`,
    ADD COLUMN `refunded_at` DATETIME NULL COMMENT 'When the refund was processed' AFTER `refund_amount`;

-- Step 2: Modify status enum to include new statuses for refund tracking
ALTER TABLE `transactions`
    MODIFY COLUMN `status` ENUM('pending', 'success', 'failed', 'expired', 'refunded', 'partially_refunded') 
    NOT NULL DEFAULT 'pending' COMMENT 'Transaction status';

-- Step 3: Add index for provider_transaction_id for faster lookups
CREATE INDEX `idx_provider_transaction_id` ON `transactions` (`provider_transaction_id`);

-- Step 4: Add index for refunded_at for analytics
CREATE INDEX `idx_refunded_at` ON `transactions` (`refunded_at`);

-- ============================================
-- Rollback Script (if needed)
-- ============================================
-- ALTER TABLE `transactions`
--     DROP COLUMN `provider_transaction_id`,
--     DROP COLUMN `provider_refund_id`,
--     DROP COLUMN `currency`,
--     DROP COLUMN `failure_reason`,
--     DROP COLUMN `payer_info`,
--     DROP COLUMN `refund_amount`,
--     DROP COLUMN `refunded_at`;
-- 
-- ALTER TABLE `transactions`
--     MODIFY COLUMN `status` ENUM('pending', 'success', 'failed', 'expired') 
--     NOT NULL DEFAULT 'pending';
-- 
-- DROP INDEX `idx_provider_transaction_id` ON `transactions`;
-- DROP INDEX `idx_refunded_at` ON `transactions`;
