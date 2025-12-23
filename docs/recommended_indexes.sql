-- ============================================
-- Database Indexes Optimization Script
-- EcomWebsite Backend
-- Generated: 2024-12-19
-- ============================================

-- NOTE: Run these on each respective database
-- Execute with caution on production - indexes can lock tables during creation

-- ============================================
-- EcomProducts Database
-- ============================================

-- Products table - commonly queried fields
-- Index for filtering by category (already exists via FK)
-- Index for filtering by manufacturer (already exists via FK)

-- Index for slug lookups (unique product pages)
CREATE UNIQUE INDEX IF NOT EXISTS idx_products_slug ON products(slug);

-- Index for filtering featured/deal/flashsale products
CREATE INDEX IF NOT EXISTS idx_products_is_feature ON products(is_feature) WHERE is_feature = 1;
CREATE INDEX IF NOT EXISTS idx_products_is_deal ON products(is_deal) WHERE is_deal = 1;
CREATE INDEX IF NOT EXISTS idx_products_is_flashsale ON products(is_flashsale) WHERE is_flashsale = 1;
CREATE INDEX IF NOT EXISTS idx_products_is_today ON products(is_today) WHERE is_today = 1;

-- Index for price range filtering
CREATE INDEX IF NOT EXISTS idx_products_price ON products(lastest_price);

-- Index for sorting by created date (new arrivals)
CREATE INDEX IF NOT EXISTS idx_products_created_at ON products(created_at DESC);

-- Composite index for common filter combinations
CREATE INDEX IF NOT EXISTS idx_products_category_price ON products(product_category_id, lastest_price);

-- Soft delete filter
CREATE INDEX IF NOT EXISTS idx_products_deleted ON products(deleted_at);

-- ProductCategories - for navigation
CREATE INDEX IF NOT EXISTS idx_categories_parent ON product_categories(parent);
CREATE INDEX IF NOT EXISTS idx_categories_deleted ON product_categories(deleted_at);

-- ProductReviews - for product ratings
CREATE INDEX IF NOT EXISTS idx_reviews_product_rating ON product_reviews(product_id, star_rating);
CREATE INDEX IF NOT EXISTS idx_reviews_user ON product_reviews(user_id);

-- ============================================
-- EcomOrders Database
-- ============================================

-- Orders table
-- Index for user's order history (already has cart_id index)
CREATE INDEX IF NOT EXISTS idx_orders_user_id ON orders(user_id);

-- Index for filtering by status
CREATE INDEX IF NOT EXISTS idx_orders_status ON orders(status);
CREATE INDEX IF NOT EXISTS idx_orders_payment_status ON orders(payment_status);

-- Index for date range queries (order reports)
CREATE INDEX IF NOT EXISTS idx_orders_created_at ON orders(created_at DESC);

-- Composite index for user + status (my orders filtering)
CREATE INDEX IF NOT EXISTS idx_orders_user_status ON orders(user_id, status);

-- Carts table
-- Index for finding active carts (already has user_id index)
CREATE INDEX IF NOT EXISTS idx_carts_user_status ON carts(user_id, status);

-- CartItems
CREATE INDEX IF NOT EXISTS idx_cart_items_product ON cart_items(product_id);

-- Transactions
-- Index for finding by status
CREATE INDEX IF NOT EXISTS idx_transactions_status ON transactions(status);

-- Index for payment method analytics
CREATE INDEX IF NOT EXISTS idx_transactions_method ON transactions(method);

-- Compound index for order + status
CREATE INDEX IF NOT EXISTS idx_transactions_order_status ON transactions(order_id, status);

-- OrderDeliveries
CREATE INDEX IF NOT EXISTS idx_deliveries_status ON order_deliveries(delivery_status);
CREATE INDEX IF NOT EXISTS idx_deliveries_tracking ON order_deliveries(tracking_code);

-- ============================================
-- EcomUsers Database  
-- ============================================

-- Users table
-- Email should already be unique, add index if not
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- Index for login queries
CREATE INDEX IF NOT EXISTS idx_users_email_active ON users(email, is_active);

-- Index for role-based queries
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role_id);

-- RefreshTokens
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires ON refresh_tokens(expires_at);

-- SocialAccounts
CREATE UNIQUE INDEX IF NOT EXISTS idx_social_provider_id ON social_accounts(provider, provider_user_id);

-- AddressBooks
CREATE INDEX IF NOT EXISTS idx_addresses_user ON address_books(user_id);
CREATE INDEX IF NOT EXISTS idx_addresses_default ON address_books(user_id, is_default);

-- Wishlists
CREATE INDEX IF NOT EXISTS idx_wishlists_user ON wishlists(user_id);
CREATE UNIQUE INDEX IF NOT EXISTS idx_wishlists_user_product ON wishlists(user_id, product_id);

-- ============================================
-- EcomBlogs Database
-- ============================================

-- Blogs table
CREATE UNIQUE INDEX IF NOT EXISTS idx_blogs_slug ON blogs(slug);
CREATE INDEX IF NOT EXISTS idx_blogs_status ON blogs(status);
CREATE INDEX IF NOT EXISTS idx_blogs_created_at ON blogs(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_blogs_author ON blogs(author_id);
CREATE INDEX IF NOT EXISTS idx_blogs_deleted ON blogs(deleted_at);

-- BlogComments
CREATE INDEX IF NOT EXISTS idx_comments_blog ON blog_comments(blog_id);
CREATE INDEX IF NOT EXISTS idx_comments_user ON blog_comments(user_id);
CREATE INDEX IF NOT EXISTS idx_comments_created ON blog_comments(created_at DESC);

-- ============================================
-- NOTES:
-- ============================================
-- 1. MySQL doesn't support "IF NOT EXISTS" for CREATE INDEX
--    Use this syntax instead: CREATE INDEX IF NOT EXISTS -> 
--    Check existence first or use ALTER TABLE with IGNORE
-- 
-- 2. Partial indexes (WHERE clause) are not supported in MySQL
--    Remove WHERE clause for MySQL compatibility
--
-- 3. For production, create indexes during low-traffic periods
--    Consider using pt-online-schema-change for large tables
--
-- 4. Monitor query performance with EXPLAIN before/after
-- ============================================
