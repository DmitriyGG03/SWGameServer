using Xunit;

namespace Tests
{
	public class AuthenticationControllerTest : BaseTest
	{
		[Theory]
		[InlineData("K4aW", "test1@gmail.com", "123456789")]
		[InlineData("MJA2K2", "test2@gmail.com", "ZfrwRgfs2")]
		[InlineData("IUR2a", "test3@gmail.com", "AFaetyew")]
		public void RegisterTest(string username, string email, string password)
		{
			Assert.NotNull(RegisterClient(username, email, password));
		}

		[Theory]
		[InlineData("Gdsr", "test11aw@gmail.com", "123456789")]
		[InlineData("ARafgdx", "test2AF@gmail.com", "ZfrwRgfs2")]
		[InlineData("Rarwa", "test3AFasf@gmail.com", "AFaetyew2")]
		public void LoginTest(string username, string email, string password)
		{
			RegisterClient(username, email, password);
			Assert.NotNull(LoginUser(email, password));
		}
	}
}
