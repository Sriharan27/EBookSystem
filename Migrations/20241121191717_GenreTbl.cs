using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBookSystem.Migrations
{
    /// <inheritdoc />
    public partial class GenreTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "BookImage",
                table: "Books",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookImage",
                table: "Books");
        }
    }
}
