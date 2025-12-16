using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NezarkaBookstore.Tests")]

namespace NezarkaBookstore
{
	public class Program
	{
		static ModelStore model;

		public static void Main(string[] args)
		{
			model = ModelStore.LoadFrom(Console.In);
			if (model == null)
			{
				Console.WriteLine("Data error.");
				return;
			}

			string line;
			while ((line = Console.ReadLine()) != null)
			{
				try
				{
					string[] parts = line.Split(' ');
					if (parts.Length != 3)
					{
						View.RenderError();
						Console.WriteLine("====");
						continue;
					}

					string verb = parts[0];
					int custId;
					if (!int.TryParse(parts[1], out custId))
					{
						View.RenderError();
						Console.WriteLine("====");
						continue;
					}
					string url = parts[2];

					if (!url.StartsWith("http://www.nezarka.net"))
					{
						View.RenderError();
						Console.WriteLine("====");
						continue;
					}

					string path = url.Substring("http://www.nezarka.net".Length);

					HandleRequest(verb, custId, path);
				}
				catch
				{
					View.RenderError();
				}

				Console.WriteLine("====");
			}
		}

		public static void HandleRequest(string verb, int customerId, string path)
		{
			if (verb != "GET")
			{
				View.RenderError();
				return;
			}

			var customer = model.GetCustomer(customerId);
			if (customer == null)
			{
				View.RenderError();
				return;
			}

			try
			{
				if (path == "/Books")
				{
					BooksController.List(model, customer);
				}
				else if (path.StartsWith("/Books/Detail/"))
				{
					string bookIdStr = path.Substring("/Books/Detail/".Length);
					int bookId;
					if (!int.TryParse(bookIdStr, out bookId))
					{
						View.RenderError();
						return;
					}
					BooksController.Detail(model, customer, bookId);
				}
				else if (path == "/ShoppingCart")
				{
					CartController.Show(model, customer);
				}
				else if (path.StartsWith("/ShoppingCart/Add/"))
				{
					string bookIdStr = path.Substring("/ShoppingCart/Add/".Length);
					int bookId;
					if (!int.TryParse(bookIdStr, out bookId))
					{
						View.RenderError();
						return;
					}
					CartController.Add(model, customer, bookId);
				}
				else if (path.StartsWith("/ShoppingCart/Remove/"))
				{
					string bookIdStr = path.Substring("/ShoppingCart/Remove/".Length);
					int bookId;
					if (!int.TryParse(bookIdStr, out bookId))
					{
						View.RenderError();
						return;
					}
					CartController.Remove(model, customer, bookId);
				}
				else
				{
					View.RenderError();
				}
			}
			catch
			{
				View.RenderError();
			}
		}
	}

	static class BooksController
	{
		public static void List(ModelStore model, Customer customer)
		{
			var books = model.GetBooks();
			View.RenderBooksList(customer, books);
		}

		public static void Detail(ModelStore model, Customer customer, int bookId)
		{
			var book = model.GetBook(bookId);

			if (book == null)
			{
				View.RenderError();
				return;
			}

			View.RenderBookDetail(customer, book);
		}
	}

	static class CartController
	{
		public static void Show(ModelStore model, Customer customer)
		{
			View.RenderShoppingCart(customer, model);
		}

		public static void Add(ModelStore model, Customer customer, int bookId)
		{
			var book = model.GetBook(bookId);

			if (book == null)
			{
				View.RenderError();
				return;
			}

			var item = customer.ShoppingCart.Items.Find(i => i.BookId == bookId);

			if (item == null)
			{
				customer.ShoppingCart.Items.Add(new ShoppingCartItem
				{
					BookId = bookId,
					Count = 1
				});
			}
			else
			{
				item.Count++;
			}

			View.RenderShoppingCart(customer, model);
		}

		public static void Remove(ModelStore model, Customer customer, int bookId)
		{
			var item = customer.ShoppingCart.Items.Find(i => i.BookId == bookId);

			if (item == null)
			{
				View.RenderError();
				return;
			}

			item.Count--;

			if (item.Count == 0)
			{
				customer.ShoppingCart.Items.Remove(item);
			}

			View.RenderShoppingCart(customer, model);
		}
	}

	class View
	{
		public static void RenderBooksList(Customer customer, IList<Book> books)
		{
			Console.WriteLine("<!DOCTYPE html>");
			Console.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
			Console.WriteLine("<head>");
			Console.WriteLine("	<meta charset=\"utf-8\" />");
			Console.WriteLine("	<title>Nezarka.net: Online Shopping for Books</title>");
			Console.WriteLine("</head>");
			Console.WriteLine("<body>");
			Console.WriteLine("	<style type=\"text/css\">");
			Console.WriteLine("		table, th, td {");
			Console.WriteLine("			border: 1px solid black;");
			Console.WriteLine("			border-collapse: collapse;");
			Console.WriteLine("		}");
			Console.WriteLine("		table {");
			Console.WriteLine("			margin-bottom: 10px;");
			Console.WriteLine("		}");
			Console.WriteLine("		pre {");
			Console.WriteLine("			line-height: 70%;");
			Console.WriteLine("		}");
			Console.WriteLine("	</style>");
			Console.WriteLine("	<h1><pre>  v,<br />Nezarka.NET: Online Shopping for Books</pre></h1>");
			Console.WriteLine("	" + customer.FirstName + ", here is your menu:");
			Console.WriteLine("	<table>");
			Console.WriteLine("		<tr>");
			Console.WriteLine("			<td><a href=\"/Books\">Books</a></td>");
			Console.WriteLine("			<td><a href=\"/ShoppingCart\">Cart (" + customer.ShoppingCart.Items.Count + ")</a></td>");
			Console.WriteLine("		</tr>");
			Console.WriteLine("	</table>");
			Console.WriteLine("	Our books for you:");
			Console.WriteLine("	<table>");

			if (books.Count > 0)
			{
				Console.WriteLine("		<tr>");
				int count = 0;
				foreach (var book in books)
				{
					if (count > 0 && count % 3 == 0)
					{
						Console.WriteLine("		</tr>");
						Console.WriteLine("		<tr>");
					}
					Console.WriteLine("			<td style=\"padding: 10px;\">");
					Console.WriteLine("				<a href=\"/Books/Detail/" + book.Id + "\">" + book.Title + "</a><br />");
					Console.WriteLine("				Author: " + book.Author + "<br />");
					Console.WriteLine("				Price: " + book.Price + " EUR &lt;<a href=\"/ShoppingCart/Add/" + book.Id + "\">Buy</a>&gt;");
					Console.WriteLine("			</td>");
					count++;
				}
				Console.WriteLine("		</tr>");
			}

			Console.WriteLine("	</table>");
			Console.WriteLine("</body>");
			Console.WriteLine("</html>");
		}

		public static void RenderBookDetail(Customer customer, Book book)
		{
			Console.WriteLine("<!DOCTYPE html>");
			Console.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
			Console.WriteLine("<head>");
			Console.WriteLine("	<meta charset=\"utf-8\" />");
			Console.WriteLine("	<title>Nezarka.net: Online Shopping for Books</title>");
			Console.WriteLine("</head>");
			Console.WriteLine("<body>");
			Console.WriteLine("	<style type=\"text/css\">");
			Console.WriteLine("		table, th, td {");
			Console.WriteLine("			border: 1px solid black;");
			Console.WriteLine("			border-collapse: collapse;");
			Console.WriteLine("		}");
			Console.WriteLine("		table {");
			Console.WriteLine("			margin-bottom: 10px;");
			Console.WriteLine("		}");
			Console.WriteLine("		pre {");
			Console.WriteLine("			line-height: 70%;");
			Console.WriteLine("		}");
			Console.WriteLine("	</style>");
			Console.WriteLine("	<h1><pre>  v,<br />Nezarka.NET: Online Shopping for Books</pre></h1>");
			Console.WriteLine("	" + customer.FirstName + ", here is your menu:");
			Console.WriteLine("	<table>");
			Console.WriteLine("		<tr>");
			Console.WriteLine("			<td><a href=\"/Books\">Books</a></td>");
			Console.WriteLine("			<td><a href=\"/ShoppingCart\">Cart (" + customer.ShoppingCart.Items.Count + ")</a></td>");
			Console.WriteLine("		</tr>");
			Console.WriteLine("	</table>");
			Console.WriteLine("	Book details:");
			Console.WriteLine("	<h2>" + book.Title + "</h2>");
			Console.WriteLine("	<p style=\"margin-left: 20px\">");
			Console.WriteLine("	Author: " + book.Author + "<br />");
			Console.WriteLine("	Price: " + book.Price + " EUR<br />");
			Console.WriteLine("	</p>");
			Console.WriteLine("	<h3>&lt;<a href=\"/ShoppingCart/Add/" + book.Id + "\">Buy this book</a>&gt;</h3>");
			Console.WriteLine("</body>");
			Console.WriteLine("</html>");
		}

		public static void RenderShoppingCart(Customer customer, ModelStore model)
		{
			var cart = customer.ShoppingCart;

			Console.WriteLine("<!DOCTYPE html>");
			Console.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
			Console.WriteLine("<head>");
			Console.WriteLine("	<meta charset=\"utf-8\" />");
			Console.WriteLine("	<title>Nezarka.net: Online Shopping for Books</title>");
			Console.WriteLine("</head>");
			Console.WriteLine("<body>");
			Console.WriteLine("	<style type=\"text/css\">");
			Console.WriteLine("		table, th, td {");
			Console.WriteLine("			border: 1px solid black;");
			Console.WriteLine("			border-collapse: collapse;");
			Console.WriteLine("		}");
			Console.WriteLine("		table {");
			Console.WriteLine("			margin-bottom: 10px;");
			Console.WriteLine("		}");
			Console.WriteLine("		pre {");
			Console.WriteLine("			line-height: 70%;");
			Console.WriteLine("		}");
			Console.WriteLine("	</style>");
			Console.WriteLine("	<h1><pre>  v,<br />Nezarka.NET: Online Shopping for Books</pre></h1>");
			Console.WriteLine("	" + customer.FirstName + ", here is your menu:");
			Console.WriteLine("	<table>");
			Console.WriteLine("		<tr>");
			Console.WriteLine("			<td><a href=\"/Books\">Books</a></td>");
			Console.WriteLine("			<td><a href=\"/ShoppingCart\">Cart (" + customer.ShoppingCart.Items.Count + ")</a></td>");
			Console.WriteLine("		</tr>");
			Console.WriteLine("	</table>");

			if (cart.Items.Count == 0)
			{
				Console.WriteLine("	Your shopping cart is EMPTY.");
				Console.WriteLine("</body>");
				Console.WriteLine("</html>");
				return;
			}

			Console.WriteLine("	Your shopping cart:");
			Console.WriteLine("	<table>");
			Console.WriteLine("		<tr>");
			Console.WriteLine("			<th>Title</th>");
			Console.WriteLine("			<th>Count</th>");
			Console.WriteLine("			<th>Price</th>");
			Console.WriteLine("			<th>Actions</th>");
			Console.WriteLine("		</tr>");

			decimal totalPrice = 0;

			foreach (var item in cart.Items)
			{
				var book = model.GetBook(item.BookId);
				if (book == null) continue;

				decimal subTotal = book.Price * item.Count;
				totalPrice += subTotal;

				Console.WriteLine("		<tr>");
				Console.WriteLine("			<td><a href=\"/Books/Detail/" + book.Id + "\">" + book.Title + "</a></td>");
				Console.WriteLine("			<td>" + item.Count + "</td>");
				if (item.Count == 1)
				{
					Console.WriteLine("			<td>" + book.Price + " EUR</td>");
				}
				else
				{
					Console.WriteLine("			<td>" + item.Count + " * " + book.Price + " = " + subTotal + " EUR</td>");
				}
				Console.WriteLine("			<td>&lt;<a href=\"/ShoppingCart/Remove/" + book.Id + "\">Remove</a>&gt;</td>");
				Console.WriteLine("		</tr>");
			}

			Console.WriteLine("	</table>");
			Console.WriteLine("	Total price of all items: " + totalPrice + " EUR");
			Console.WriteLine("</body>");
			Console.WriteLine("</html>");
		}

		public static void RenderError()
		{
			Console.WriteLine("<!DOCTYPE html>");
			Console.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
			Console.WriteLine("<head>");
			Console.WriteLine("	<meta charset=\"utf-8\" />");
			Console.WriteLine("	<title>Nezarka.net: Online Shopping for Books</title>");
			Console.WriteLine("</head>");
			Console.WriteLine("<body>");
			Console.WriteLine("<p>Invalid request.</p>");
			Console.WriteLine("</body>");
			Console.WriteLine("</html>");
		}
	}

	class ModelStore
	{
		private List<Book> books = new List<Book>();
		private List<Customer> customers = new List<Customer>();

		public IList<Book> GetBooks()
		{
			return books;
		}

		public Book GetBook(int id)
		{
			return books.Find(b => b.Id == id);
		}

		public Customer GetCustomer(int id)
		{
			return customers.Find(c => c.Id == id);
		}

		public static ModelStore LoadFrom(TextReader reader)
		{
			var store = new ModelStore();

			try
			{
				string line = reader.ReadLine();
				if (line != "DATA-BEGIN")
				{
					return null;
				}

				while (true)
				{
					line = reader.ReadLine();
					if (line == null)
					{
						return null;
					}
					else if (line == "DATA-END")
					{
						break;
					}

					string[] tokens = line.Split(';');
					if (tokens.Length == 0)
					{
						return null;
					}

					switch (tokens[0])
					{
						case "BOOK":
							if (tokens.Length != 5)
							{
								return null;
							}
							store.books.Add(new Book
							{
								Id = int.Parse(tokens[1]),
								Title = tokens[2],
								Author = tokens[3],
								Price = decimal.Parse(tokens[4])
							});
							break;
						case "CUSTOMER":
							if (tokens.Length != 4)
							{
								return null;
							}
							store.customers.Add(new Customer
							{
								Id = int.Parse(tokens[1]),
								FirstName = tokens[2],
								LastName = tokens[3]
							});
							break;
						case "CART-ITEM":
							if (tokens.Length != 4)
							{
								return null;
							}
							var customer = store.GetCustomer(int.Parse(tokens[1]));
							if (customer == null)
							{
								return null;
							}
							customer.ShoppingCart.Items.Add(new ShoppingCartItem
							{
								BookId = int.Parse(tokens[2]),
								Count = int.Parse(tokens[3])
							});
							break;
						default:
							return null;
					}
				}
			}
			catch (Exception ex)
			{
				if (ex is FormatException || ex is IndexOutOfRangeException)
				{
					return null;
				}
				throw;
			}

			return store;
		}
	}

	class Book
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Author { get; set; }
		public decimal Price { get; set; }
	}

	class Customer
	{
		private ShoppingCart shoppingCart;

		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public ShoppingCart ShoppingCart
		{
			get
			{
				if (shoppingCart == null)
				{
					shoppingCart = new ShoppingCart();
				}
				return shoppingCart;
			}
			set
			{
				shoppingCart = value;
			}
		}
	}

	class ShoppingCartItem
	{
		public int BookId { get; set; }
		public int Count { get; set; }
	}

	class ShoppingCart
	{
		public int CustomerId { get; set; }
		public List<ShoppingCartItem> Items = new List<ShoppingCartItem>();
	}
}