using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bike_Service_IMS.Data
{
	public class ItemRequest
	{
		[JsonIgnore]
		private static int nextId = 1;
		public int Id { get; set; }
		public string Name { get; set; }
		public int Quantity { get; set; }
		public DateTime RequestedDate { get; set; } = DateTime.Now;
		public DateTime ApprovedDate { get; set; }
		public string RequestedBy { get; set; }
		public string ApprovedBy { get; set; }
		public bool IsApproved { get; set; } = false;
		public ItemRequest()
		{
			Id = nextId++;
		}
	}
}
