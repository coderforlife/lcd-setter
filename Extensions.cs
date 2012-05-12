namespace System
{
    public delegate void Action();
    //public delegate void Action<T>(T x); // exists in 2.0
    public delegate void Action<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d); // exists in 2.0
    public delegate R Func<R>();
    public delegate R Func<R, T>(T x);
    public delegate R Func<R, T1, T2, T3>(T1 a, T2 b, T3 c);
}
