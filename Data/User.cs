
namespace Bike_Service_IMS.Data
{
	public class User
	{
		public string Fullname { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
