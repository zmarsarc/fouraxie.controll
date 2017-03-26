using System;
using System.Runtime;

namespace FlyWind.DataStructs.Point {
    interface IVector3 {
        double x { get; set; }
        double y { get; set; }
        double z { get; set; }
    }

    interface IVector2 {
        double x { get; set; }
        double y { get; set; }
    }

    class Vector3 : IVector3 {

        public static readonly Vector3 zero = new Vector3();
        public static readonly Vector3 up = new Vector3(0f, 0f, 1f);
        public static readonly Vector3 down = -up;
        public static readonly Vector3 left = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 right = -left;
        public static readonly Vector3 forward = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 backward = -forward;

        public double x { set; get; }
        public double y { set; get; }
        public double z { set; get; }

        public Vector3(double x = 0f, double y = 0f, double z = 0f) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3(IVector3 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public double distanceFrom(double x, double y, double z) {
            return Math.Sqrt(Math.Pow(this.x - x, 2f) + Math.Pow(this.y - y, 2f) + Math.Pow(this.z - z, 2f));
        }

        public double distanceFrom(IVector3 vec3) {
            return this.distanceFrom(vec3.x, vec3.y, vec3.z);
        }

        #region reload operator *
        public static Vector3 operator *(double num, Vector3 vec3) {
            return new Vector3(vec3.x * num, vec3.y * num, vec3.z * num);
        }
        public static Vector3 operator *(Vector3 vec3, double num) {
            return 3 * vec3;
        }
        public static Vector3 operator /(Vector3 vec3, double num) {
            return vec3 * (1 / num);
        }
        #endregion reload operator *

        #region reload operator +
        public static Vector3 operator + (Vector3 a, Vector3 b) {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3 operator - (Vector3 a) {
            return new Vector3(-a.x, -a.y, -a.z);
        }
        public static Vector3 operator - (Vector3 a, Vector3 b) {
            return a + (-b);
        }
        #endregion reload operator +

        #region reload operator ==
        public static bool operator ==(Vector3 a, Vector3 b) {
            return (a.x == b.x && a.y == b.y && a.z == b.z);
        }
        public static bool operator !=(Vector3 a, Vector3 b) {
            return !(a == b);
        }
        #endregion reload operator ==

        public override bool Equals(object obj) {
            return this == (Vector3)obj;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public double dot(Vector3 vec) {
            return (this.x * vec.x + this.y * vec.y + this.z * vec.z);
        }

        public Vector3 cross(Vector3 vec) {
            return new Vector3(y * vec.z - z * vec.y, z * vec.x - x * vec.z, x * vec.y - vec.x * y);
        }

        public double mode() {
            return Math.Sqrt(Math.Pow(x, 2f) + Math.Pow(y, 2f) + Math.Pow(z, 2f));
        }

        public Vector3 direction() {
            return (this / mode());
        }
    }

    class Vector2 : IVector2 {

        public static readonly Vector2 zero = new Vector2();
        public static readonly Vector2 up = new Vector2(0f, 1f);
        public static readonly Vector2 down = -up;
        public static readonly Vector2 right = new Vector2(1f, 0f);
        public static readonly Vector2 left = -right;

        public double x { set; get; }
        public double y { set; get; }

        public Vector2(double x = 0f, double y = 0f) {
            this.x = x;
            this.y = y;
        }
        public Vector2(IVector2 v) {
            this.x = v.x;
            this.y = v.y;
        }

        public double distanceFrom(double x, double y) {
            return Math.Sqrt(Math.Pow(this.x - x, 2f) + Math.Pow(this.y - y, 2f));
        }

        public double distanceFrom(IVector2 vec2) {
            return this.distanceFrom(vec2.x, vec2.y);
        }

        #region reload operator *
        public static Vector2 operator *(double num, Vector2 vec2) {
            return new Vector2(vec2.x * num, vec2.y * num);
        }
        public static Vector2 operator *(Vector2 vec2, double num) {
            return num * vec2;
        }
        public static Vector2 operator /(Vector2 vec2, double num) {
            return vec2 * (1 / num);
        }
        #endregion reload operator *

        #region reload operator +
        public static Vector2 operator +(Vector2 a, Vector2 b) {
            return new Vector2(a.x * b.x, a.y * b.y);
        }
        public static Vector2 operator -(Vector2 a) {
            return new Vector2(-a.x, -a.y);
        }
        public static Vector2 operator -(Vector2 a, Vector2 b) {
            return a + (-b);
        }
        #endregion reload operator +

        #region reload operator ==
        public static bool operator ==(Vector2 a, Vector2 b) {
            return (a.x == b.x && a.y == b.y);
        }
        public static bool operator !=(Vector2 a, Vector2 b) {
            return !(a == b);
        }
        #endregion reload operator ==

        public override bool Equals(object obj) {
            return this == (Vector2)obj;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <summary>
        /// 求本向量和 vec 向量的叉积
        /// </summary>
        /// <param name="vec">指定向量</param>
        /// <returns></returns>
        public double croee(Vector2 vec) {
            return (this.x * vec.y - this.y * vec.x);
        }

        /// <summary>
        /// 求本向量和 vec 向量的点积
        /// </summary>
        /// <param name="vec">指定向量</param>
        /// <returns></returns>
        public double dot(Vector2 vec) {
            return (this.x * vec.x + this.y * vec.y);
        }

        public double mode() {
            return Math.Sqrt(Math.Pow(x, 2f) + Math.Pow(y, 2f));
        }

        public Vector2 direction() {
            return (this / mode());
        }
    }
}
