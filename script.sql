USE [master]
GO

CREATE DATABASE [EShop]
GO

USE [EShop]
GO

CREATE TABLE [dbo].[Users](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[username] [varchar](128) NULL,
	[email] [varchar](128) NOT NULL,
	[password] [varchar](128) NOT NULL,
	[access_level] [varchar](32) NOT NULL,
	[verified] [bit] NOT NULL,
 	CONSTRAINT [PK_Users] PRIMARY KEY([id])
)

CREATE TABLE [dbo].[Products](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](512) NOT NULL,
	[price] [float] NOT NULL,
	[number] [int] NOT NULL,
	[image_url] [text] NOT NULL,
	[description] [text] NULL,
 	CONSTRAINT [PK_Products] PRIMARY KEY([id])
)
GO

CREATE TABLE [dbo].[Categories](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](128) NOT NULL,
	[description] [text] NULL,
	CONSTRAINT [PK_Categories] PRIMARY KEY([id])
)
GO

CREATE TABLE [dbo].[Orders](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[total_cost] [money] NOT NULL,
	[status] [varchar](32) NOT NULL,
 	CONSTRAINT [PK_Orders] PRIMARY KEY([id]),
	CONSTRAINT [FK_Orders_Users] FOREIGN KEY([user_id]) REFERENCES [dbo].[Users] ([id]) ON DELETE CASCADE
)
GO

CREATE TABLE [dbo].[OrderItems](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[order_id] [int] NOT NULL,
	[product_id] [int] NOT NULL,
	[number] [int] NOT NULL,
 	CONSTRAINT [PK_OrderItems] PRIMARY KEY([id]),
	CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY([order_id]) REFERENCES [dbo].[Orders] ([id]) ON DELETE CASCADE,
	CONSTRAINT [FK_OrderItems_Products] FOREIGN KEY([product_id]) REFERENCES [dbo].[Products] ([id]) ON DELETE CASCADE
)
GO

CREATE TABLE [dbo].[CartItems](
	[user_id] [int] NOT NULL,
	[product_id] [int] NOT NULL,
	[number] [int] NOT NULL,
	CONSTRAINT [PK_CartItems] PRIMARY KEY([user_id], [product_id]),
 	CONSTRAINT [FK_CartItems_Products] FOREIGN KEY([product_id]) REFERENCES [dbo].[Products] ([id]) ON DELETE CASCADE,
	CONSTRAINT [FK_CartItems_Users] FOREIGN KEY([user_id]) REFERENCES [dbo].[Users] ([id]) ON DELETE CASCADE
)
GO

CREATE TABLE [dbo].[Product_Category](
	[product_id] [int] NOT NULL,
	[category_id] [int] NOT NULL,
 	CONSTRAINT [PK_Product_Category] PRIMARY KEY([product_id], [category_id]),
	CONSTRAINT [FK_Product_Category_Categories] FOREIGN KEY([category_id]) REFERENCES [dbo].[Categories] ([id]) ON DELETE CASCADE,
	CONSTRAINT [FK_Product_Category_Products] FOREIGN KEY([product_id]) REFERENCES [dbo].[Products] ([id]) ON DELETE CASCADE
)
GO

CREATE TABLE [dbo].[ProductKeys](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[product_id] [int] NOT NULL,
	[value] [varchar](50) NOT NULL,
 	CONSTRAINT [PK_ProductKeys] PRIMARY KEY([id]),
	CONSTRAINT [FK_ProductKeys_Products] FOREIGN KEY([product_id]) REFERENCES [dbo].[Products] ([id]) ON DELETE CASCADE
)
GO

CREATE TABLE [dbo].[UserConfirmKeys](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[secret_key] [varchar](64) NOT NULL,
 	CONSTRAINT [PK_UserConfirmKeys] PRIMARY KEY([user_id]),
	CONSTRAINT [FK_UserConfirmKeys_Users] FOREIGN KEY([user_id]) REFERENCES [dbo].[Users] ([id])
)
GO