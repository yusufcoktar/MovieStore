using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalMovieStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActorModelToSingleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Actors");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Actors",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Actors",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Actors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
