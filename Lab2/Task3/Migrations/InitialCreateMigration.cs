using FluentMigrator;

namespace Task3.Migrations;

[Migration(20241018)]
public class InitialCreateMigration : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
                CREATE TABLE IF NOT EXISTS products
                (
                    product_id    BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    product_name  TEXT NOT NULL,
                    product_price MONEY NOT NULL
                );

                CREATE TYPE order_state AS ENUM ('created', 'processing', 'completed', 'cancelled');

                CREATE TABLE IF NOT EXISTS orders
                (
                    order_id         BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    order_state      order_state              NOT NULL,
                    order_created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                    order_created_by TEXT                     NOT NULL
                );

                CREATE TABLE IF NOT EXISTS order_items
                (
                    order_item_id       BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    order_id            BIGINT  NOT NULL REFERENCES orders (order_id),
                    product_id          BIGINT  NOT NULL REFERENCES products (product_id),
                    order_item_quantity INT     NOT NULL,
                    order_item_deleted  BOOLEAN NOT NULL
                );

                CREATE TYPE order_history_item_kind AS ENUM ('created', 'item_added', 'item_removed', 'state_changed');

                CREATE TABLE IF NOT EXISTS order_history
                (
                    order_history_item_id         BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    order_id                      BIGINT                   NOT NULL REFERENCES orders (order_id),
                    order_history_item_created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                    order_history_item_kind       order_history_item_kind  NOT NULL,
                    order_history_item_payload    JSONB                    NOT NULL
                );
            ");
    }

    public override void Down()
    {
        Execute.Sql(@"
                DROP TABLE IF EXISTS order_history;
                DROP TABLE IF EXISTS order_items;
                DROP TABLE IF EXISTS orders;
                DROP TABLE IF EXISTS products;
                
                DROP TYPE IF EXISTS order_history_item_kind;
                DROP TYPE IF EXISTS order_state;
            ");
    }
}
