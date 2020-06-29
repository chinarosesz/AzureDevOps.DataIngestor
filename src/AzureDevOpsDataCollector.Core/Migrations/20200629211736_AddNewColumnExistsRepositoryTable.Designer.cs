﻿// <auto-generated />
using System;
using AzureDevOpsDataCollector.Core.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AzureDevOpsDataCollector.Core.Migrations
{
    [DbContext(typeof(VssDbContext))]
    [Migration("20200629211736_AddNewColumnExistsRepositoryTable")]
    partial class AddNewColumnExistsRepositoryTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AzureDevOpsDataCollector.Core.Entities.VssProjectEntity", b =>
                {
                    b.Property<Guid>("ProjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Exists")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<long>("Revision")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("State")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Visibility")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ProjectId");

                    b.HasIndex("OrganizationName")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("VssProject");
                });

            modelBuilder.Entity("AzureDevOpsDataCollector.Core.Entities.VssRepositoryEntity", b =>
                {
                    b.Property<Guid>("RepoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DefaultBranch")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Exists")
                        .HasColumnType("bit");

                    b.Property<string>("OrganizationName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProjectName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepoName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("WebUrl")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RepoId");

                    b.HasIndex("OrganizationName")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("VssRepository");
                });
#pragma warning restore 612, 618
        }
    }
}
