using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended
{
    /// <summary>
    ///     Base class for shapes.
    /// </summary>
    /// <remakarks>
    ///     Created to allow checking intersection between shapes of different types.
    /// </remakarks>
    public interface IShapeF
    {
        /// <summary>
        /// Gets or sets the position of the shape.
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Gets escribed rectangle, which lying outside the shape
        /// </summary>
        RectangleF BoundingRectangle { get; }
    }

    /// <summary>
    ///     Class that implements methods for shared <see cref="IShapeF" /> methods.
    /// </summary>
    public static class Shape
    {
        /// <summary>
        ///     Check if two shapes intersect.
        /// </summary>
        /// <param name="a">The first shape.</param>
        /// <param name="b">The second shape.</param>
        /// <returns>True if the two shapes intersect.</returns>
        public static bool Intersects(this IShapeF a, IShapeF b)
        {
            switch (a)
            {
                case RectangleF rectangleA when b is RectangleF rectangleB:
                    return rectangleA.Intersects(rectangleB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(a));
            }
        }
    }
}
