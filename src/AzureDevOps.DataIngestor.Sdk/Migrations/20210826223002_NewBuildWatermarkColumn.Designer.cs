﻿// <auto-generated />
using System;
using AzureDevOps.DataIngestor.Sdk.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AzureDevOps.DataIngestor.Sdk.Migrations
{
    [DbContext(typeof(VssDbContext))]
    [Migration("20210826223002_NewBuildWatermarkColumn")]
    partial class NewBuildWatermarkColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AzureDevOps.DataIngestor.Sdk.Entities.VssBuildDefinitionEntity", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("GZipCompressedJsonData")
                        .HasColumnType("varbinary(max)");

                    b.Property<bool?>("IsHosted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Path")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PoolId")
                        .HasColumnType("int");

                    b.Property<string>("PoolName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Process")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("QueueId")
                        .HasColumnType("int");

                    b.Property<string>("QueueName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepositoryId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RepositoryName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("UniqueName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WebLink")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id", "ProjectId");

                    b.ToTable("VssBuildDefinition");
                });

            modelBuilder.Entity("AzureDevOps.DataIngestor.Sdk.Entities.VssBuildDefinitionStepEntity", b =>
                {
                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BuildDefinitionId")
                        .HasColumnType("int");

                    b.Property<int>("StepNumber")
                        .HasColumnType("int");

                    b.Property<string>("Condition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Enabled")
                        .HasColumnType("bit");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhaseName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PhaseQueueId")
                        .HasColumnType("int");

                    b.Property<string>("PhaseRefName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhaseType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("TaskDefinitionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("TaskVersionSpec")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ProjectId", "BuildDefinitionId", "StepNumber");

                    b.ToTable("VssBuildDefinitionStep");
                });

            modelBuilder.Entity("AzureDevOps.DataIngestor.Sdk.Entities.VssBuildEntity", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BuildNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DefinitionId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("FinishTime")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("KeepForever")
                        .HasColumnType("bit");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("QueueId")
                        .HasColumnType("int");

                    b.Property<string>("QueueName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("QueueTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("RepositoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("Result")
                        .HasColumnType("int");

                    b.Property<bool?>("RetainedByRelease")
                        .HasColumnType("bit");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SourceBranch")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SourceVersion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id", "ProjectId");

                    b.ToTable("VssBuild");
                });

            modelBuilder.Entity("AzureDevOps.DataIngestor.Sdk.Entities.VssBuildWatermarkEntity", b =>
                {
                    b.Property<Guid>("ProjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("LatestBuildFinishTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ProjectId");

                    b.ToTable("VssBuildWatermarkEntities");
                });

            modelBuilder.Entity("AzureDevOps.DataIngestor.Sdk.Entities.VssProjectEntity", b =>
                {
                    b.Property<Guid>("ProjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

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

                    b.HasIndex("Organization");

                    b.ToTable("VssProject");
                });

            modelBuilder.Entity("AzureDevOps.DataIngestor.Sdk.Entities.VssPullRequestEntity", b =>
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

            modelBuilder.Entity("AzureDevOps.DataIngestor.Sdk.Entities.VssPullRequestWatermarkEntity", b =>
                {
                    b.Property<string>("PullRequestStatus")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RowUpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("PullRequestStatus", "ProjectId");

                    b.ToTable("VssPullRequestWatermark");
                });

            modelBuilder.Entity("AzureDevOps.DataIngestor.Sdk.Entities.VssRepositoryEntity", b =>
                {
                    b.Property<Guid>("RepoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

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

                    b.HasIndex("Organization");

                    b.ToTable("VssRepository");
                });
#pragma warning restore 612, 618
        }
    }
}
