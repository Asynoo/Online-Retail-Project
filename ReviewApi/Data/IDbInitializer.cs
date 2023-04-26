namespace ReviewApi.Data {
    public interface IDbInitializer {
        void Initialize(ReviewApiContext context);
    }
}
