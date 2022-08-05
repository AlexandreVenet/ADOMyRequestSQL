using ADOMyRequestSQL.Helper;

namespace ADOMyRequestSQL
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Title("Exemples d'utilisation de MyRequest");

			////////////////////////////////////////////////////////////////// 

			//////////////////////////////////////////////////////////////////
			// A FAIRE : anonymiser la chaîne de recherche dans MyRequest.cs
			// ... et supprimer ce bloc...
			//////////////////////////////////////////////////////////////////

			SubTitle("A. Capture d'exceptions");

			Step("A.1. Adresse serveur erronnée ou serveur inaccessible", CheckConnection);

			Step("A.2. Requête erronnée", CheckQuery);

			SubTitle("B. Requêtes uniques");

			Step("B.1. Lire tous les enregistrements", SelectAll);

			Step("B.2. Lire un enregistrement à l'id 2", SelectById);

			SubTitle("C. Requêtes multiples");

			Step("C.1 Insérer un enregistrement puis le lire", Insert);

			Step("C.2 Lire le dernier enregistrement, le mettre à jour, le lire", Update);

			Step("C.3 Insérer un enregistrement et le supprimer", Delete);

			//////////////////////////////////////////////////////////////////
			
			Title("Fin de programme");
			Console.Read();
		}



		#region MyRequest methods

		// Capture d'exceptions

		private static void CheckConnection()
		{
			try
			{
				using(var mr = new MyRequest())
				{
					Console.WriteLine("\tConnexion établie");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"\tErreur de connexion\n\t{e.GetType()}\n\t{e.Message}");
				// Microsoft.Data.SqlClient.SqlException
			}
		}

		private static void CheckQuery()
		{
			try
			{
				using (var mr = new MyRequest())
				{
					object prenomNom = null;
					int id = 1;
					prenomNom = mr.RequestScalar("SELECT prenom + ' ' + nom AS prenom_nom FROM Test WHERE xxxNIMPORTEQUOIxxx=@0",
						new object[] { id });
					if (prenomNom == null)
					{
						Console.WriteLine($"\tErreur");
						return;
					}
					Console.WriteLine($"\tLe prénom et nom à l'id {id} sont : {(string)prenomNom}");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"\tErreur de requête\n\t{e.GetType()}\n\t{e.Message}");
				// Microsoft.Data.SqlClient.SqlException
			}
		}

		// Par souci de clarté, les blocs try...catch ne sont pas repris dans les exemples suivants.

		// Requêtes uniques

		private static void SelectAll()
		{
			using (MyRequest mr = new())
			{
				// Pas de paramètres nommés
				mr.RequestReader("SELECT id, prenom, nom, inscription FROM Test", (reader) =>
				{
					Console.WriteLine($"\t{reader.GetInt32(0)}, {reader.GetString(1)}, {reader.GetString(2)}, {DateOnly.FromDateTime(reader.GetDateTime(3))}");
				});
			}
		}

		private static void SelectById()
		{
			using (MyRequest mr = new())
			{
				// Paramètre nommé positionnel
				mr.RequestReader("SELECT id, prenom, nom, inscription FROM Test WHERE id=@0", (reader) =>
				{
					Console.WriteLine($"\t{reader.GetInt32(0)}, {reader.GetString(1)} {reader.GetString(2)}, {DateOnly.FromDateTime(reader.GetDateTime(3))}");
				},
				new object[] { 2 });
			}
		}

		// Requêtes multiples

		private static void Insert()
		{
			using(MyRequest mr = new())
			{
				object data = null;
				// Paramètres nommés placeholders
				data = mr.RequestScalar("INSERT INTO Test (inscription, prenom, nom) OUTPUT INSERTED.id VALUES(@ins, @pre, @nom)", 
					new MyRequestPlaceholder[]
					{ 
						new ("ins", new DateTime(3000,12,01)),
						new ("pre", "Youpi"),
						new ("nom", "Lavie"),
					});

				if (data == null)
				{
					Console.WriteLine("\tErreur");
				}
				else
				{
					int id = (int)data;
					Console.WriteLine($"\tInsertion effectuée. Id de l'enregistrement : {id}.");

					mr.RequestReader("SELECT id, prenom, nom, inscription FROM Test WHERE id=@0", (reader) =>
					{
						Console.WriteLine($"\t{reader.GetInt32(0)}, {reader.GetString(1)} {reader.GetString(2)}, {DateOnly.FromDateTime(reader.GetDateTime(3))}");
					},
					new object[] { id });
				}
			}
		}

		private static void Update()
		{
			using(MyRequest mr = new())
			{
				object id = mr.RequestScalar("SELECT MAX(id) FROM Test");

				if(id == null)
				{
					Console.WriteLine("\tErreur");
					return;
				}

				int intId = (int)id;

				Console.WriteLine($"\tLe dernier id de la table est : {intId}. Voici l'enregistrement :");

				mr.RequestReader("SELECT nom, prenom, inscription FROM Test WHERE id=@intID", (reader) => 
				{
					Console.WriteLine($"\t{intId}, {reader.GetString(1)}, {reader.GetString(0)}, {DateOnly.FromDateTime(reader.GetDateTime(2))}");
				}, 
				new MyRequestPlaceholder[] { new("intID",intId) });

				int lines = mr.RequestNonQuery("UPDATE Test SET nom=@nom, prenom=@prenom, inscription=@inscription WHERE id=@id", 
					new MyRequestPlaceholder[] 
					{
						new("id", intId),
						new("inscription", DateTime.Now),
						new("prenom", "MODIF"),
						new("nom", "MODIF"),
					});

				Console.WriteLine($"\n\tL'enregistrement est modifié. Voici les nouvelles valeurs :");

				mr.RequestReader("SELECT nom, prenom, inscription FROM Test WHERE id=@0", (reader) =>
				{
					Console.WriteLine($"\t{intId}, {reader.GetString(1)}, {reader.GetString(0)}, {DateOnly.FromDateTime(reader.GetDateTime(2))}");
				},
				new object[] { intId });
			}
		}

		private static void Delete()
		{
			using(var mr = new MyRequest())
			{
				object data = null;
				
				data = mr.RequestScalar("INSERT INTO Test (inscription, prenom, nom) OUTPUT INSERTED.id VALUES(@ins, @pre, @nom)",
					new MyRequestPlaceholder[]
					{
						new ("ins", DateTime.Now),
						new ("pre", "Last"),
						new ("nom", "NotLeast"),
					});

				if (data == null)
				{
					Console.WriteLine("\tErreur");
					return;
				}

				int id = (int) data;

				Console.WriteLine($"\tInsertion effectuée. Nouvel enregistrement à l'id : {id}.");

				Console.WriteLine("\n\tLes données sont les suivantes : ");

				mr.RequestReader("SELECT prenom + ' ' + nom AS prenom_nom, inscription FROM Test WHERE id=@0", (reader) =>
				{
					Console.WriteLine($"\t{id}, {reader.GetString(0)}, {DateOnly.FromDateTime(reader.GetDateTime(1))}");
				}, new object[] { id });

				object deleted = mr.RequestScalar("DELETE FROM Test OUTPUT DELETED.id WHERE id=@0",
					new object[] { id });

				if(deleted == null)
				{
					Console.WriteLine("\n\tErreur suppression");
					return;
				}

				Console.WriteLine($"\n\tL'enregistrement à l'id {(int)deleted} a été supprimé.");
			}
		}

		#endregion



		#region Program methods (UI, controls...)

		private static void Title(string str)
		{
			int n = 50;
			char c = '─';
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine(new string(c,n));
			Console.WriteLine(str);
			Console.WriteLine(new string(c,n));
			Console.WriteLine();
			Console.ResetColor();
		}

		private static void SubTitle(string str)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine(str + "\n");
			Console.ResetColor();
		}

		private static bool WaitUser()
		{
			bool result = false;
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(" [ENTREE : Lancer][ECHAP : Passer] ");
			while (true)
			{
				ConsoleKey key = Console.ReadKey(true).Key;
				if (key == ConsoleKey.Enter)
				{
					result = true;
					break;
				}
				else if (key == ConsoleKey.Escape)
				{
					result = false;
					break;
				}
			}
			Console.WriteLine("\n");
			Console.ResetColor();
			return result;
		}

		private static void Step(string title, Action act)
		{
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write(title);
			Console.ResetColor();
			if (WaitUser())
			{
				act();
				Console.WriteLine();
			}
			else
			{
				Console.WriteLine("\tInstruction passée\n");
			}
		}

		#endregion
	}
}