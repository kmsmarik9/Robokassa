namespace KmsDev.Robokassa
{
    public enum RobokassaHashAlgorithm
    {
        Md5,
        Ripemd160,
        Sha1,
        Sha256,
        Sha384,
        Sha512
    }

    public enum RobokassaErrorKind
    {
        Http,
        Transport,
        Timeout,
        Api,
        InvalidResponse,
        InvalidSignature
    }

    public enum RobokassaHost
    {
        Services,
        Merchant
    }

    public enum RobokassaPasswordKind
    {
        Payment,
        Management,
        Notification,
        Refund
    }
}
