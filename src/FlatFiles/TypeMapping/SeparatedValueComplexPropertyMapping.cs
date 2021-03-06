﻿using System;
using System.Reflection;
using FlatFiles.Resources;

namespace FlatFiles.TypeMapping
{
    /// <summary>
    /// Represents the mapping from a type property to an object.
    /// </summary>
    public interface ISeparatedValueComplexPropertyMapping
    {
        /// <summary>
        /// Sets the name of the column in the input or output file.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The property mapping for further configuration.</returns>
        ISeparatedValueComplexPropertyMapping ColumnName(string name);

        /// <summary>
        /// Sets the options to use when reading/writing the complex type.
        /// </summary>
        /// <param name="options">The options to use.</param>
        /// <returns>The property mapping for further configuration.</returns>
        ISeparatedValueComplexPropertyMapping WithOptions(SeparatedValueOptions options);

        /// <summary>
        /// Sets the value to treat as null.
        /// </summary>
        /// <param name="value">The value to treat as null.</param>
        /// <returns>The property mapping for further configuration.</returns>
        ISeparatedValueComplexPropertyMapping NullValue(string value);

        /// <summary>
        /// Sets a custom handler for nulls.
        /// </summary>
        /// <param name="handler">The handler to use to recognize nulls.</param>
        /// <returns>The property mapping for further configuration.</returns>
        /// <remarks>Setting the handler to null with use the default handler.</remarks>
        ISeparatedValueComplexPropertyMapping NullHandler(INullHandler handler);

        /// <summary>
        /// Sets a function to preprocess in the input before parsing it.
        /// </summary>
        /// <param name="preprocessor">A preprocessor function.</param>
        /// <returns>The property mapping for further configuration.</returns>
        ISeparatedValueComplexPropertyMapping Preprocessor(Func<string, string> preprocessor);
    }

    internal sealed class SeparatedValueComplexPropertyMapping<TEntity> : ISeparatedValueComplexPropertyMapping, IPropertyMapping
    {
        private readonly ISeparatedValueTypeMapper<TEntity> mapper;
        private readonly PropertyInfo property;
        private string columnName;
        private SeparatedValueOptions options;
        private INullHandler nullHandler;
        private Func<string, string> preprocessor;

        public SeparatedValueComplexPropertyMapping(ISeparatedValueTypeMapper<TEntity> mapper, PropertyInfo property)
        {
            this.mapper = mapper;
            this.property = property;
            this.columnName = property.Name;
        }

        public IColumnDefinition ColumnDefinition
        {
            get
            {
                SeparatedValueSchema schema = mapper.GetSchema();
                SeparatedValueComplexColumn column = new SeparatedValueComplexColumn(columnName, schema);
                column.Options = options;
                column.NullHandler = nullHandler;
                column.Preprocessor = preprocessor;

                var recordMapper = (IRecordMapper<TEntity>)mapper;
                return new ComplexMapperColumn<TEntity>(column, recordMapper);
            }
        }

        public PropertyInfo Property
        {
            get { return property; }
        }

        public ISeparatedValueComplexPropertyMapping ColumnName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(SharedResources.BlankColumnName);
            }
            this.columnName = name;
            return this;
        }

        public ISeparatedValueComplexPropertyMapping WithOptions(SeparatedValueOptions options)
        {
            this.options = options;
            return this;
        }

        public ISeparatedValueComplexPropertyMapping NullHandler(INullHandler handler)
        {
            this.nullHandler = handler;
            return this;
        }

        public ISeparatedValueComplexPropertyMapping NullValue(string value)
        {
            this.nullHandler = new ConstantNullHandler(value);
            return this;
        }

        public ISeparatedValueComplexPropertyMapping Preprocessor(Func<string, string> preprocessor)
        {
            this.preprocessor = preprocessor;
            return this;
        }
    }
}
