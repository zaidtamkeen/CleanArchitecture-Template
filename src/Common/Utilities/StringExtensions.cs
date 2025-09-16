using System;

namespace CleanTemplate.Common.Utilities
{
    /// <summary>
    /// أساليب ملحقة لمساعدة التعامل مع السلاسل والقيم الرقمية (تحويل/تنسيق/تحقق).
    /// تُستخدم عبر الطبقات للتحقق من المدخلات وتكوين مخرجات قابلة للقراءة.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// يتحقق إن كانت السلسلة تحتوي قيمة (مع خيار تجاهل المسافات البيضاء).
        /// </summary>
        /// <param name="value">السلسلة المراد فحصها.</param>
        /// <param name="ignoreWhiteSpace">عند true تُعتبر المسافات البيضاء فارغة.</param>
        /// <returns>true عند وجود قيمة؛ false خلاف ذلك.</returns>
        public static bool HasValue(this string value, bool ignoreWhiteSpace = true)
        {
            return ignoreWhiteSpace ? !string.IsNullOrWhiteSpace(value) : !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// يحوّل السلسلة إلى عدد صحيح 32-بت.
        /// </summary>
        /// <param name="value">نص رقمي متوقع.</param>
        /// <returns>قيمة int المحوّلة.</returns>
        /// <exception cref="FormatException">عند نص غير رقمي.</exception>
        /// <exception cref="OverflowException">عند تجاوز المجال.</exception>
        public static int ToInt(this string value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// يحوّل السلسلة إلى قيمة عشرية (decimal).
        /// </summary>
        /// <param name="value">نص رقمي عشري.</param>
        /// <returns>قيمة decimal المحوّلة.</returns>
        /// <exception cref="FormatException">عند نص غير رقمي.</exception>
        /// <exception cref="OverflowException">عند تجاوز المجال.</exception>
        public static decimal ToDecimal(this string value)
        {
            return Convert.ToDecimal(value);
        }

        /// <summary>
        /// ينسّق عددًا صحيحًا باستخدام فواصل الآلاف حسب الثقافة الحالية.
        /// </summary>
        /// <param name="value">قيمة int.</param>
        /// <returns>نص رقمي منسّق (مثال: 1,234).</returns>
        public static string ToNumeric(this int value)
        {
            return value.ToString("N0"); //"123,456"
        }

        /// <summary>
        /// ينسّق قيمة عشرية باستخدام فواصل الآلاف بدون منازل عشرية.
        /// </summary>
        /// <param name="value">قيمة decimal.</param>
        /// <returns>نص رقمي منسّق.</returns>
        public static string ToNumeric(this decimal value)
        {
            return value.ToString("N0");
        }

        /// <summary>
        /// ينسّق عددًا صحيحًا كعملة حسب الثقافة الحالية.
        /// </summary>
        /// <param name="value">قيمة int.</param>
        /// <returns>نص منسّق كعملة (مثل 123,123﷼).</returns>
        public static string ToCurrency(this int value)
        {
            //fa-IR => current culture currency symbol => ریال
            //123456 => "123,123ریال"
            return value.ToString("C0");
        }

        /// <summary>
        /// ينسّق قيمة عشرية كعملة حسب الثقافة الحالية.
        /// </summary>
        /// <param name="value">قيمة decimal.</param>
        /// <returns>نص منسّق كعملة.</returns>
        public static string ToCurrency(this decimal value)
        {
            return value.ToString("C0");
        }

        /// <summary>
        /// يعيد null إذا كانت السلسلة فارغة لتبسيط الحقول الاختيارية.
        /// </summary>
        /// <param name="str">السلسلة المرشّحة.</param>
        /// <returns>null عند الفراغ؛ وإلا السلسلة الأصلية.</returns>
        public static string NullIfEmpty(this string str)
        {
            return str?.Length == 0 ? null : str;
        }
    }
}
