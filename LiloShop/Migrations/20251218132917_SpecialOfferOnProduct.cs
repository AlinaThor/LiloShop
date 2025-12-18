using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiloShop.Migrations
{
    /// <inheritdoc />
    public partial class SpecialOfferOnProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSpecialOffer",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSpecialOffer",
                table: "Products");
        }
    }
}
