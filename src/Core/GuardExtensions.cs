namespace Ardalis.GuardClauses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public static class CollectionGuardExtensions
    {
        /// <summary>
        /// Throws an exception if the provided collection does not contain the specified item.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <typeparam name="TItem">The type of the item to be checked</typeparam>
        /// <param name="guardClause">The guard clause instance</param>
        /// <param name="collection">The collection to be checked</param>
        /// <param name="item">The item that should be present in the collection</param>
        /// <param name="parameterName">Name of the parameter (automatically filled by the compiler)</param>
        /// <exception cref="ArgumentException">Thrown if the collection does not contain the item</exception>
        /// <exception cref="ArgumentNullException">Thrown if the collection is null</exception>
        public static void NotContains<T, TItem>(this IGuardClause guardClause,
            IEnumerable<T> collection,
            TItem item,
            [CallerArgumentExpression("collection")] string? parameterName = null)
            where TItem : T
        {
            if (collection == null)
                throw new ArgumentNullException(parameterName);

            if (!collection.Contains(item))
                throw new ArgumentException($"Collection does not contain the required item: {item}", parameterName);
        }

        /// <summary>
        /// Throws an exception if the provided collection does not contain the specified item using a custom comparer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <typeparam name="TItem">The type of the item to be checked</typeparam>
        /// <param name="guardClause">The guard clause instance</param>
        /// <param name="collection">The collection to be checked</param>
        /// <param name="item">The item that should be present in the collection</param>
        /// <param name="comparer">The custom equality comparer to check for equality</param>
        /// <param name="parameterName">Name of the parameter (automatically filled by the compiler)</param>
        /// <exception cref="ArgumentException">Thrown if the collection does not contain the item</exception>
        /// <exception cref="ArgumentNullException">Thrown if the collection is null</exception>
        public static void NotContains<T, TItem>(this IGuardClause guardClause,
            IEnumerable<T> collection,
            TItem item,
            IEqualityComparer<T> comparer,
            [CallerArgumentExpression("collection")] string? parameterName = null)
            where TItem : T
        {
            if (collection == null)
                throw new ArgumentNullException(parameterName);

            if (!collection.Contains(item, comparer))
                throw new ArgumentException($"Collection does not contain the required item: {item}", parameterName);
        }
    }
}