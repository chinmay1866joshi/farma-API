-- ============================================================================
-- Kafh Organic Farming Platform - Complete PostgreSQL Database Backup & Seed
-- Database Name: farma_db
-- Encoding: UTF-8
-- Compatible with: PostgreSQL 14, 15, 16, 17, 18
-- Generated At: 2026-10-24
-- ============================================================================

-- If you are restoring in pgAdmin Query Tool or psql as postgres superuser,
-- you can run the following two lines first if the database does not exist:
--
-- CREATE DATABASE farma_db WITH OWNER = postgres ENCODING = 'UTF8';
-- \c farma_db;

SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

START TRANSACTION;

-- ============================================================================
-- 1. DROP EXISTING TABLES (Reverse Dependency Order)
-- ============================================================================
DROP TABLE IF EXISTS "OrderItems" CASCADE;
DROP TABLE IF EXISTS "Orders" CASCADE;
DROP TABLE IF EXISTS "Products" CASCADE;
DROP TABLE IF EXISTS "Farms" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;
DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;

-- ============================================================================
-- 2. CREATE EF MIGRATIONS HISTORY TABLE
-- ============================================================================
CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- ============================================================================
-- 3. CREATE USERS TABLE
-- ============================================================================
CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Email" character varying(150) NOT NULL,
    "PasswordHash" text NOT NULL,
    "MobileNumber" character varying(15) NOT NULL,
    "Age" integer NOT NULL,
    "Role" character varying(50) NOT NULL,
    "City" character varying(100) NOT NULL,
    "State" character varying(100) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Users_Age" CHECK ("Age" >= 18 AND "Age" <= 120)
);

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

-- ============================================================================
-- 4. CREATE FARMS TABLE
-- ============================================================================
CREATE TABLE "Farms" (
    "Id" uuid NOT NULL,
    "FarmerId" uuid NOT NULL,
    "FarmName" character varying(150) NOT NULL,
    "Location" character varying(200) NOT NULL,
    "TotalAcres" numeric(18,2) NOT NULL,
    "SoilType" character varying(100),
    "OrganicCertificationNo" character varying(100),
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_Farms" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Farms_Users_FarmerId" FOREIGN KEY ("FarmerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Farms_FarmerId" ON "Farms" ("FarmerId");

-- ============================================================================
-- 5. CREATE PRODUCTS TABLE
-- ============================================================================
CREATE TABLE "Products" (
    "Id" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Description" character varying(1000) NOT NULL DEFAULT '',
    "Category" character varying(100) NOT NULL,
    "PricePerUnit" numeric(18,2) NOT NULL,
    "Unit" character varying(20) NOT NULL DEFAULT 'kg',
    "QuantityAvailable" numeric(18,2) NOT NULL,
    "ImageUrl" character varying(500),
    "IsOrganic" boolean NOT NULL DEFAULT true,
    "FarmerId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Products" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Products_Users_FarmerId" FOREIGN KEY ("FarmerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Products_FarmerId" ON "Products" ("FarmerId");
CREATE INDEX "IX_Products_Category" ON "Products" ("Category");

-- ============================================================================
-- 6. CREATE ORDERS TABLE
-- ============================================================================
CREATE TABLE "Orders" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "OrderDate" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "TotalAmount" numeric(18,2) NOT NULL,
    "Status" character varying(50) NOT NULL DEFAULT 'Pending',
    "DeliveryAddress" character varying(250) NOT NULL,
    "ContactPhone" character varying(15) NOT NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Orders_Users_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");

-- ============================================================================
-- 7. CREATE ORDER ITEMS TABLE
-- ============================================================================
CREATE TABLE "OrderItems" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Quantity" numeric(18,2) NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "TotalPrice" numeric(18,2) NOT NULL,
    CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
CREATE INDEX "IX_OrderItems_ProductId" ON "OrderItems" ("ProductId");

-- ============================================================================
-- 8. SEED DATA - EF MIGRATION HISTORY
-- ============================================================================
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261023191647_InitialCreate', '10.0.13');

-- ============================================================================
-- 9. SEED DATA - USERS
-- (All default users share the password: "Password123!")
-- PasswordHash generated via BCrypt.Net: $2a$11$5WD3xgIKxF1gSlRD2xxFfOcFSXvr/7.Wbjuhu.ftGopKKlz18smYe
-- ============================================================================
INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "MobileNumber", "Age", "Role", "City", "State", "CreatedAt")
VALUES 
    (
        '11111111-1111-1111-1111-111111111111',
        'Ramesh Patil',
        'ramesh.farmer@example.com',
        '$2a$11$5WD3xgIKxF1gSlRD2xxFfOcFSXvr/7.Wbjuhu.ftGopKKlz18smYe',
        '9876543210',
        38,
        'farmer',
        'Pune',
        'Maharashtra',
        NOW() - INTERVAL '10 days'
    ),
    (
        '22222222-2222-2222-2222-222222222222',
        'Suresh Deshmukh',
        'suresh.farmer@example.com',
        '$2a$11$5WD3xgIKxF1gSlRD2xxFfOcFSXvr/7.Wbjuhu.ftGopKKlz18smYe',
        '9822334455',
        45,
        'farmer',
        'Nashik',
        'Maharashtra',
        NOW() - INTERVAL '8 days'
    ),
    (
        '33333333-3333-3333-3333-333333333333',
        'Ananya Sharma',
        'ananya.customer@example.com',
        '$2a$11$5WD3xgIKxF1gSlRD2xxFfOcFSXvr/7.Wbjuhu.ftGopKKlz18smYe',
        '9123456780',
        29,
        'customer',
        'Mumbai',
        'Maharashtra',
        NOW() - INTERVAL '5 days'
    ),
    (
        '44444444-4444-4444-4444-444444444444',
        'Vikram Joshi',
        'vikram.customer@example.com',
        '$2a$11$5WD3xgIKxF1gSlRD2xxFfOcFSXvr/7.Wbjuhu.ftGopKKlz18smYe',
        '9871122334',
        32,
        'customer',
        'Pune',
        'Maharashtra',
        NOW() - INTERVAL '3 days'
    ),
    (
        '99999999-9999-9999-9999-999999999999',
        'Admin Kafh',
        'admin@kafhfarm.com',
        '$2a$11$5WD3xgIKxF1gSlRD2xxFfOcFSXvr/7.Wbjuhu.ftGopKKlz18smYe',
        '9000000000',
        40,
        'admin',
        'Pune',
        'Maharashtra',
        NOW() - INTERVAL '30 days'
    );

-- ============================================================================
-- 10. SEED DATA - FARMS
-- ============================================================================
INSERT INTO "Farms" ("Id", "FarmerId", "FarmName", "Location", "TotalAcres", "SoilType", "OrganicCertificationNo", "CreatedAt")
VALUES
    (
        'fa111111-1111-1111-1111-111111111111',
        '11111111-1111-1111-1111-111111111111',
        'Green Valley Organic Agro',
        'Dapoli, Ratnagiri, Maharashtra',
        14.50,
        'Red Laterite (High Iron & Organic Matter)',
        'NPOP/NAB/0019/2024',
        NOW() - INTERVAL '10 days'
    ),
    (
        'fa222222-2222-2222-2222-222222222222',
        '22222222-2222-2222-2222-222222222222',
        'Sahyadri Hill Terrace Organics',
        'Trimbakeshwar, Nashik, Maharashtra',
        22.00,
        'Black Regur / Volcanic Loam',
        'PGS-INDIA/2023/8892',
        NOW() - INTERVAL '8 days'
    );

-- ============================================================================
-- 11. SEED DATA - PRODUCTS (CROPS & ORGANIC PRODUCE)
-- ============================================================================
INSERT INTO "Products" ("Id", "Name", "Description", "Category", "PricePerUnit", "Unit", "QuantityAvailable", "ImageUrl", "IsOrganic", "FarmerId", "CreatedAt")
VALUES
    (
        'a1111111-1111-1111-1111-111111111111',
        'Organic Alphonso Mangoes (Hapus)',
        'GI-tagged, naturally ripened, zero chemical pesticides from coastal Ratnagiri orchards.',
        'Fruits',
        450.00,
        'kg',
        140.00,
        '/images/products/alphonso_mango.jpg',
        true,
        '11111111-1111-1111-1111-111111111111',
        NOW() - INTERVAL '5 days'
    ),
    (
        'a2222222-2222-2222-2222-222222222222',
        'Fresh Organic Spinach (Palak)',
        'Crisp, tender, farm-harvested baby spinach leaves grown with vermicompost.',
        'Vegetables',
        35.00,
        'bunch',
        85.00,
        '/images/products/spinach.jpg',
        true,
        '11111111-1111-1111-1111-111111111111',
        NOW() - INTERVAL '4 days'
    ),
    (
        'a3333333-3333-3333-3333-333333333333',
        'Vine-Ripened Desi Tomatoes',
        'Juicy, heirloom variety tomatoes with rich natural acidity and deep red color.',
        'Vegetables',
        50.00,
        'kg',
        200.00,
        '/images/products/tomatoes.jpg',
        true,
        '22222222-2222-2222-2222-222222222222',
        NOW() - INTERVAL '4 days'
    ),
    (
        'a4444444-4444-4444-4444-444444444444',
        'Aromatic Traditional Brown Basmati Rice',
        'Single-origin unpolished long-grain organic basmati, aged naturally for 12 months.',
        'Grains',
        160.00,
        'kg',
        450.00,
        '/images/products/brown_basmati.jpg',
        true,
        '22222222-2222-2222-2222-222222222222',
        NOW() - INTERVAL '6 days'
    ),
    (
        'a5555555-5555-5555-5555-555555555555',
        'Fresh A2 Gir Cow Raw Milk',
        'Pure whole milk from free-grazing Gir indigenous cows, non-homogenized, chilled on farm.',
        'Dairy',
        85.00,
        'litre',
        60.00,
        '/images/products/a2_milk.jpg',
        true,
        '11111111-1111-1111-1111-111111111111',
        NOW() - INTERVAL '1 days'
    ),
    (
        'a6666666-6666-6666-6666-666666666666',
        'Pure Raw Wildflower Honey',
        'Unpasteurized, unprocessed multifloral honey collected sustainably from Western Ghats apiaries.',
        'Organic Pantry',
        620.00,
        'kg',
        35.00,
        '/images/products/wildflower_honey.jpg',
        true,
        '22222222-2222-2222-2222-222222222222',
        NOW() - INTERVAL '7 days'
    ),
    (
        'a7777777-7777-7777-7777-777777777777',
        'Fresh High-Curcumin Salem Turmeric',
        'Golden root turmeric rich in aromatic essential oils and 5.2% curcumin content.',
        'Herbs',
        120.00,
        'kg',
        70.00,
        '/images/products/turmeric.jpg',
        true,
        '11111111-1111-1111-1111-111111111111',
        NOW() - INTERVAL '3 days'
    ),
    (
        'a8888888-8888-8888-8888-888888888888',
        'Crisp Sweet Baby Carrots',
        'Sweet, freshly pulled crunchy carrots, washed and packed without chemicals.',
        'Vegetables',
        60.00,
        'kg',
        110.00,
        '/images/products/carrots.jpg',
        true,
        '22222222-2222-2222-2222-222222222222',
        NOW() - INTERVAL '2 days'
    );

-- ============================================================================
-- 12. SEED DATA - SAMPLE CUSTOMER ORDER
-- ============================================================================
INSERT INTO "Orders" ("Id", "CustomerId", "OrderDate", "TotalAmount", "Status", "DeliveryAddress", "ContactPhone")
VALUES
    (
        'b1111111-1111-1111-1111-111111111111',
        '33333333-3333-3333-3333-333333333333',
        NOW() - INTERVAL '2 days',
        1030.00,
        'Confirmed',
        'Flat 402, Green Acre Heights, Senapati Bapat Road, Pune, Maharashtra 411016',
        '9123456780'
    );

INSERT INTO "OrderItems" ("Id", "OrderId", "ProductId", "Quantity", "UnitPrice", "TotalPrice")
VALUES
    (
        'c1111111-1111-1111-1111-111111111111',
        'b1111111-1111-1111-1111-111111111111',
        'a1111111-1111-1111-1111-111111111111',
        2.00,
        450.00,
        900.00
    ),
    (
        'c2222222-2222-2222-2222-222222222222',
        'b1111111-1111-1111-1111-111111111111',
        'a2222222-2222-2222-2222-222222222222',
        2.00,
        35.00,
        70.00
    ),
    (
        'c3333333-3333-3333-3333-333333333333',
        'b1111111-1111-1111-1111-111111111111',
        'a8888888-8888-8888-8888-888888888888',
        1.00,
        60.00,
        60.00
    );

COMMIT;

-- ============================================================================
-- End of Database Backup
-- ============================================================================
