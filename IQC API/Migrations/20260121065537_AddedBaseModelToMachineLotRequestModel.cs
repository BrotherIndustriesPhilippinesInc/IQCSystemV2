using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class AddedBaseModelToMachineLotRequestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WhatFor",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "WhatFor",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "WhatFor",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "WhatFor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MachineLotRequest",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "MachineLotRequest",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MachineLotRequest",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "MachineLotRequest",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WhatFor");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "WhatFor");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "WhatFor");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "WhatFor");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MachineLotRequest");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "MachineLotRequest");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MachineLotRequest");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "MachineLotRequest");
        }
    }
}
