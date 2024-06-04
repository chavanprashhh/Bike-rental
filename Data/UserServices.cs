
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;


namespace Bike_Service_IMS.Data
{

    public static class UserServices
    {
        public const string SeedUsername = "admin";
        public const string SeedPassword = "123456";
        public static string GetAppDirectoryPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Bike-Service"
            );
        }
        public static string GetUsersPath()
        {
            return Path.Combine(GetAppDirectoryPath(), "users.json");
        }
        public static string GetItemsPath()
        {
            return Path.Combine(GetAppDirectoryPath(), "items.json");
        }
        public static string GetItemsRecordPath()
        {
            return Path.Combine(GetAppDirectoryPath(), "itemsRecord.json");
        }
        public static void SeedUsers()
        {
            var users = Read().FirstOrDefault(x => x.Role.Equals("Admin"));

            if (users == null)
            {
                Add("Unknown", "Admin", SeedUsername, SeedPassword, "Admin");
            }
        }
        public static User Login(string username, string password)
        {
            var loginErrorMessage = "Invalid username or password.";
            List<User> users = Read();
            User user = users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                throw new Exception(loginErrorMessage);
            }
            if (!user.Password.Equals(password))
            {
                throw new Exception(loginErrorMessage);
            }
            return user;
        }
        //---------------------------------------------------------------------------------------------------------------
        // For Users
        public static List<User> Read()
        {
            string path = GetUsersPath();
            if (!File.Exists(path))
            {
                return new List<User>();
            }

            string readJson = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<User>>(readJson);
        }
        private static void SaveAll(List<User> users)
        {
            string appDataDirectoryPath = GetAppDirectoryPath();
            if (!Directory.Exists(appDataDirectoryPath))
            {
                Directory.CreateDirectory(appDataDirectoryPath);
            }
            string path = GetUsersPath();
            var json = JsonSerializer.Serialize(users);
            File.WriteAllText(path, json);
        }

        public static List<User> Add(string createdBy, string fullname, string username, string password, string role)
        {
            List<User> users = Read();
            bool usernameExists = users.Any(x => x.Username == username);

            if (usernameExists)
            {
                throw new Exception("Username already exists!");
            }
            users.Add(
                new User
                {
                    Fullname = fullname,
                    Username = username,
                    Password = password,
                    Role = role,
                    CreatedBy = createdBy
                }
            );
            SaveAll(users);
            return users;
        }

        public static List<User> Delete(string username)
        {
            List<User> users = Read();
            User user = users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                throw new Exception("User not found.");
            }
            users.Remove(user);
            SaveAll(users);
            return users;
        }
        //---------------------------------------------------------------------------------------------------------------
        // For Item
        public static List<Item> ReadItem()
        {
            string path = GetItemsPath();
            if (!File.Exists(path))
            {
                return new List<Item>();
            }

            string readJson = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Item>>(readJson);
        }
        private static void SaveAllItem(List<Item> items)
        {
            string appDataDirectoryPath = GetAppDirectoryPath();
            if (!Directory.Exists(appDataDirectoryPath))
            {
                Directory.CreateDirectory(appDataDirectoryPath);
            }
            string path = GetItemsPath();
            var json = JsonSerializer.Serialize(items);
            File.WriteAllText(path, json);
        }

        public static List<Item> AddItem(string name, int quantity, DateTime addedDate, string addedBy, double purchasePrice, double sellingPrice)
        {
            List<Item> items = ReadItem();
            bool itemExists = items.Any(x => x.Name == name);

            if (itemExists)
            {
                throw new Exception("There is a item with the same name!");
            }
            items.Add(
                new Item
                {
                    Name = name,
                    Quantity = quantity,
                    AddedDate = addedDate,
                    AddedBy = addedBy,
                    PurchasePrice = purchasePrice,
                    SellingPrice = sellingPrice
                }
            );
            SaveAllItem(items);
            return items;
        }
        public static List<Item> DeleteItem(string name)
        {
            List<Item> items = ReadItem();
            Item item = items.FirstOrDefault(x => x.Name == name);
            if (item == null)
            {
                throw new Exception("Item not found.");
            }
            items.Remove(item);
            SaveAllItem(items);
            return items;
        }
        public static List<Item> UpdateItem(string name,int quantity)
        {
            List<Item> items = ReadItem();
            Item item = items.FirstOrDefault(x => x.Name == name);
            if (item == null)
            {
                throw new Exception("Item not found.");
            }
            item.Quantity += quantity;
            SaveAllItem(items);
            return items;
        }
        //---------------------------------------------------------------------------------------------------------------
        // For Item requests
        public static List<ItemRequest> ReadItemReq()
        {
            string path = GetItemsRecordPath();
            if (!File.Exists(path))
            {
                return new List<ItemRequest>();
            }

            string readJson = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<ItemRequest>>(readJson);
        }
        private static void SaveAllItemReq(List<ItemRequest> itemsRequest)
        {
            string appDataDirectoryPath = GetAppDirectoryPath();
            if (!Directory.Exists(appDataDirectoryPath))
            {
                Directory.CreateDirectory(appDataDirectoryPath);
            }
            string path = GetItemsRecordPath();
            var json = JsonSerializer.Serialize(itemsRequest);
            File.WriteAllText(path, json);
        }

        public static List<ItemRequest> AddItemReq(string name, int quantity, string requestedBy)
        {
            List<ItemRequest> itemsRequests = ReadItemReq();
            List<Item> items = ReadItem();
            Item item = items.FirstOrDefault(x => x.Name == name);
            if (item.Quantity < quantity)
            {
                throw new Exception("There is not enough quantity in the inventory currently!");
            }
            itemsRequests.Add(
                new ItemRequest
                {
                    Name = name,
                    Quantity = quantity,
                    RequestedBy = requestedBy,
                }
            );
            SaveAllItemReq(itemsRequests);
            return itemsRequests;
        }
        public static List<ItemRequest> Approve(int id, string name, string approvedBy, DateTime approvedDate, bool isApproved)
        {
            List<ItemRequest> itemRequests = ReadItemReq();
            ItemRequest itemRequest = itemRequests.FirstOrDefault(x => x.Id == id);
            List<Item> items = ReadItem();
            Item item = items.FirstOrDefault(x => x.Name == name);
            if (item == null)
            {
                itemRequests = DeleteItemReq(id);
                throw new Exception("The item has been removed from the inventory!");
            }
            int reducedStock = item.Quantity - itemRequest.Quantity;
            item.Quantity = reducedStock;
            SaveAllItem(items);
            itemRequest.ApprovedBy = approvedBy;
            itemRequest.ApprovedDate = approvedDate;
            itemRequest.IsApproved = isApproved;
            SaveAllItemReq(itemRequests);
            return itemRequests;
        }

        public static void AddOtherItemDetail(string name, DateTime lastApprove, int quantity)
        {
            List<Item> items = ReadItem();
            Item item = items.FirstOrDefault(x => x.Name == name);
            item.LastApprovedDate = lastApprove;
            item.TotalQuantityTaken += quantity;
            SaveAllItem(items);
        }
        public static List<ItemRequest> DeleteItemReq(int id)
        {
            List<ItemRequest> itemRequests = ReadItemReq();
            ItemRequest itemRequest = itemRequests.FirstOrDefault(x => x.Id == id);
            if (itemRequest == null)
            {
                throw new Exception("Item Request not found.");
            }
            itemRequests.Remove(itemRequest);
            SaveAllItemReq(itemRequests);
            return itemRequests;
        }

        // For Other Functions
        public static List<double> GetChartData()
        {
            List<Item> items = ReadItem();
            List<double> quantity = new();
            foreach (var item in items)
            {
                if (item.TotalQuantityTaken > 0)
                {
                    quantity.Add(item.TotalQuantityTaken);
                }

            }

            return quantity;
        }
        public static List<string> GetChartLabel()
        {
            List<Item> items = ReadItem();
            List<string> name = new();
            foreach (var item in items)
            {
                if (item.TotalQuantityTaken > 0)
                {
                    name.Add(item.Name);
                }
            }
            return name;
        }
        public static bool CheckTime()
        {
            bool isOpen;
            string[] workingDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
            TimeOnly currentTime = TimeOnly.FromDateTime(DateTime.Now);
            DateTime currentFullTime = DateTime.Now;
            string weekday = currentFullTime.DayOfWeek.ToString();
            TimeOnly openTime = TimeOnly.Parse("9:00 AM");
            TimeOnly closeTime = TimeOnly.Parse("5:00 PM");
            if (currentTime.CompareTo(openTime) >= 0 && currentTime.CompareTo(closeTime) <= 0 && workingDays.Contains(weekday))
            {
                isOpen = true;
            }
            else
            {
                isOpen = false;
            }
            return isOpen;
        }
    }

}
