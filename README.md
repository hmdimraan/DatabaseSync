# DatabaseSync

A C# / .NET console application that synchronizes data from a SQL Server database to a MariaDB database using schema comparison and hash-based change detection.

## Overview

DatabaseSync is designed to keep a MariaDB database synchronized with a SQL Server source database.

Instead of copying data on every synchronization cycle, the application compares database structures and calculates SHA-256 hashes of table data to determine whether changes have occurred. Data is synchronized only when a change is detected.

## Key Features

- SQL Server as the source database
- MariaDB as the target database
- Automatic schema comparison
- SHA-256 hash-based change detection
- Table-level data synchronization
- Configurable synchronization interval
- Synchronization logging
- Persistent hash storage
- MariaDB-compatible date/time handling
- Configuration through `appsettings.json`
- Separation of database access and synchronization responsibilities

## Architecture

```text
                    SQL Server
                  Source Database
                        │
                        ▼
              ┌───────────────────┐
              │  SchemaComparer   │
              │ Schema Comparison │
              └─────────┬─────────┘
                        │
                        ▼
              ┌───────────────────┐
              │ DataSynchronizer  │
              │ Change Detection  │
              │ & Data Sync       │
              └───────┬─────┬─────┘
                      │     │
             ┌────────▼─┐ ┌─▼────────┐
             │HashStorage│ │  Logger  │
             │SHA-256    │ │Activity  │
             │Tracking   │ │Tracking  │
             └───────────┘ └──────────┘
                      │
                      ▼
                  MariaDB
               Target Database
