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
    [Migration("20200702174722_UpdatePullRequestWatermarkTable")]
    partial class UpdatePullRequestWatermarkTable
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

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Organization")
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

                    b.HasIndex("Organization")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("VssProject");
                });

            modelBuilder.Entity("AzureDevOpsDataCollector.Core.Entities.VssPullRequestEntity", b =>
                {
                    b.Property<int>("PullRequestId")
                        .HasColumnType("int");

                    b.Property<Guid>("RepositoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AuthorEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ClosedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastMergeCommitID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastMergeTargetCommitId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProjectName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SourceBranch")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TargetBranch")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PullRequestId", "RepositoryId");

                    b.ToTable("VssPullRequest");
                });

            modelBuilder.Entity("AzureDevOpsDataCollector.Core.Entities.VssPullRequestWatermarkEntity", b =>
                {
                    b.Property<Guid>("RepositoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("MostRecentDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProjectName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PullRequestId")
                        .HasColumnType("int");

                    b.Property<string>("PullRequestStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepositoryName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("RepositoryId");

                    b.ToTable("VssPullRequestWatermark");
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

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProjectName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("WebUrl")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RepoId");

                    b.HasIndex("Organization")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("VssRepository");
                });
#pragma warning restore 612, 618
        }
    }
}
