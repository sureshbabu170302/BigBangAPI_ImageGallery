﻿// <auto-generated />
using ImageGallery.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ImageGallery.Migrations
{
    [DbContext(typeof(ImageGalleryContext))]
    partial class ImageGalleryContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ImageGallery.Models.Gallery", b =>
                {
                    b.Property<int>("Image_Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Image_Id"));

                    b.Property<int>("Admin_Id")
                        .HasColumnType("int");

                    b.Property<string>("Gallery_Image")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Image_Id");

                    b.ToTable("Galleries");
                });
#pragma warning restore 612, 618
        }
    }
}
