// Copyright (c)  Maikebing. All rights reserved.
// Licensed under the MIT License, See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using IoTSharp.EntityFrameworkCore.Taos.Diagnostics.Internal;
using IoTSharp.EntityFrameworkCore.Taos.Infrastructure.Internal;
using IoTSharp.EntityFrameworkCore.Taos.Internal;
using IoTSharp.EntityFrameworkCore.Taos.Migrations.Internal;
using IoTSharp.EntityFrameworkCore.Taos.Query.Internal;
using IoTSharp.EntityFrameworkCore.Taos.Storage.Internal;
using IoTSharp.EntityFrameworkCore.Taos.Update.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Taos specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class TaosServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the Taos database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Calling this method is no longer necessary when building most applications, including those that
        ///         use dependency injection in ASP.NET or elsewhere.
        ///         It is only needed when building the internal service provider for use with
        ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        ///         This is not recommend other than for some advanced scenarios.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkTaos([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, TaosLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<TaosOptionsExtension>>()
                .TryAdd<IDatabase, TaosDatabase>()//没有用。。
                .TryAdd<IRelationalTypeMappingSource, TaosTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, TaosSqlGenerationHelper>()
                .TryAdd<IMigrationsAnnotationProvider, TaosMigrationsAnnotationProvider>()
                .TryAdd<IModelValidator, TaosModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, TaosConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator, TaosUpdateSqlGenerator>()
                .TryAdd<IModificationCommandBatchFactory, TaosModificationCommandBatchFactory>()
                .TryAdd<IRelationalConnection>(p => p.GetService<ITaosRelationalConnection>())
                .TryAdd<IMigrationsSqlGenerator, TaosMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, TaosDatabaseCreator>()
                .TryAdd<IHistoryRepository, TaosHistoryRepository>()
                .TryAdd<IBatchExecutor, TaosBatchExecutor>()
                .TryAdd<ICommandBatchPreparer, TaosCommandBatchPreparer>()
                .TryAdd<IQueryContextFactory, TaosQueryContextFactory>()
                .TryAdd<IModelCustomizer, TaosModelCustomizer>()
                .TryAdd<IRelationalCommandBuilderFactory, TaosEFCommandBuilderFactory>()

                // New Query Pipeline
                .TryAdd<IMethodCallTranslatorProvider, TaosMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, TaosMemberTranslatorProvider>()
                .TryAdd<IQuerySqlGeneratorFactory, TaosQuerySqlGeneratorFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, TaosQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, TaosSqlTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalParameterBasedSqlProcessorFactory, TaosParameterBasedSqlProcessorFactory>()

                .TryAddProviderSpecificServices(
                    b => b
                    .TryAddScoped<IQueryProvider, TaosQueryProvider>()
                    .TryAddScoped<ITaosRelationalConnection, TaosRelationalConnection>()
                    .TryAddScoped<IRelationalCommandBuilder, TaosEFCommandBuilder>()//配置无效，未创建
                    .TryAddScoped<IRelationalCommand, TaosEFCommand>()//配置无效，未创建
                    )

                .TryAdd<ISqlExpressionFactory, TaosSqlExpressionFactory>()//表达式的生成
                .TryAdd<IRelationalQueryStringFactory, TaosRelationalQueryStringFactory>()



            ;

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
