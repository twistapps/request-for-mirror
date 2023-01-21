namespace RequestForMirror
{
    public class RequestId
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public int ID;

        internal RequestId(int id)
        {
            ID = id;
        }

        public RequestId Next() 
        {
            ID++;
            return new RequestId(ID);
        }

        public static implicit operator RequestId(int n)
        {
            return new RequestId(n);
        }

        public static explicit operator int(RequestId request)
        {
            return request.ID;
        }
    }
}