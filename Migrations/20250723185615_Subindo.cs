using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Subindo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_message_date",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "last_message_is_reply",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "last_message_is_seen",
                table: "chats");

            migrationBuilder.RenameColumn(
                name: "last_message_text",
                table: "chats",
                newName: "client_email");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_updated",
                table: "sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_cpf_cnpj",
                table: "chats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_messages",
                table: "chats",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_created",
                table: "agents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_updated",
                table: "agents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "agents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "real_whatsapp_number",
                table: "agents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_updated",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "client_cpf_cnpj",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "last_messages",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "date_created",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "date_updated",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "name",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "real_whatsapp_number",
                table: "agents");

            migrationBuilder.RenameColumn(
                name: "client_email",
                table: "chats",
                newName: "last_message_text");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_message_date",
                table: "chats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "last_message_is_reply",
                table: "chats",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "last_message_is_seen",
                table: "chats",
                type: "boolean",
                nullable: true);
        }
    }
}
