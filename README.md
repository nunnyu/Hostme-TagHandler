## Overview

This is an AI-powered analytics system designed to process restaurant booking receipts and generate intelligent customer insights. The application automatically categorizes and tags customer orders using Azure OpenAI, enabling data-driven customer segmentation and marketing intelligence.

The system processes booking messages from Azure Service Bus, retrieves receipt data from cloud storage, analyzes order content using GPT-4, and maintains comprehensive customer profiles with tag-based analytics in a PostgreSQL database.

## Technical Architecture

Built with .NET 8.0, the application implements a modular architecture with the following components:

- **Receiver Service**: Azure Service Bus message consumption and Azure Blob Storage integration for receipt retrieval
- **AI Analysis Engine**: Azure OpenAI (GPT-4.1) integration for intelligent tag generation from receipt items
- **Data Aggregation Layer**: Customer profile building with order statistics and tag aggregation
- **Database Layer**: PostgreSQL integration with Dapper ORM for persistent customer analytics storage
- **Message Processing**: Batch processing pipeline for receipt analysis and customer profile updates

## Key Features

- Real-time message processing from Azure Service Bus queues
- AI-powered receipt analysis using Azure OpenAI GPT-4.1
- Automated tag generation and categorization of restaurant orders
- Customer profile aggregation across multiple orders with statistical analysis
- PostgreSQL database integration for customer tags, preferences, and order history
- Multi-language receipt processing support
- Natural language search query conversion for customer segmentation

## Technologies and Services

- **Framework**: .NET 8.0 (C#)
- **Cloud Services**: Azure Service Bus, Azure OpenAI Service, Azure Blob Storage
- **Database**: PostgreSQL with Dapper ORM
- **AI/ML**: Azure OpenAI GPT-4.1 for natural language processing and categorization
- **Libraries**: Azure.Messaging.ServiceBus, Azure.AI.OpenAI, Npgsql, Microsoft.Extensions.Configuration

## Problem Statement

The system addresses the challenge of automatically extracting meaningful insights from restaurant receipt data at scale. By leveraging AI to analyze order content and generate contextual tags, the application enables hospitality businesses to understand customer preferences, identify dining patterns, and support targeted marketing initiatives without manual data entry or categorization.
