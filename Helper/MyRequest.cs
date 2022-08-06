using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOMyRequestSQL.Helper
{
	/// <summary>
	/// Classe concrète pour effectuer des requêtes préparées Transact-SQL à partir de fonctions factorisées, avec le pattern Dispose.
	/// </summary>
	internal class MyRequest : IDisposable
	{
		#region Fields

		/// <summary>
		/// Chaîne de connexion
		/// </summary>
		private string _adressDB = @"Data Source=(LocalDB)\NomInstance;Initial Catalog=NomBDD;Integrated Security=True";

		/// <summary>
		/// L'objest est-il en cours de nettoyage ?
		/// </summary>
		private bool isDisposed;

		/// <summary>
		/// La connexion à la base de données.
		/// </summary>
		private SqlConnection _connection;

		/// <summary>
		/// La commande SQL.
		/// </summary>
		private SqlCommand _sqlCommand;

		#endregion



		#region Constructors

		/// <summary>
		/// Constructeur. Utiliser avec using(...){...} ou appeler Dispose() pour profiter du pattern.
		/// </summary>
		public MyRequest()
		{
			// Définir la connexion et ouvrir
			_connection = new SqlConnection(_adressDB);
			_connection.Open();
		}

		#endregion



		#region Dispose pattern

		/// <summary>
		/// Dispose() appelle Dispose(true)
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Le nettoyage s'effectue ici avec le paramètre true. Fonction appelée par Dispose().
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (isDisposed) return;

			if (disposing)
			{
				// Ressources managées à supprimer

				_connection.Close(); 
			}

			isDisposed = true;
		}

		/// <summary>
		/// Destructeur/Finaliseur
		/// </summary>
		~MyRequest()
		{
			// GC passe par ici et appelle Dispose(false)
			Dispose(false);
		}

		#endregion



		#region Methods : params & prepare

		/// <summary>
		/// Traitement des paramètres et préparation de la commande. 
		/// <br/>ADO gère la reconnaissance des types.
		/// <br/>Paramètres nommés par position.
		/// </summary>
		/// <param name="cmd">La commande SQL à préparer.</param>
		/// <param name="args">Tableau des valeurs (Object) pour les paramètres nommés.</param>
		private void ParametersPrepareByPosition(SqlCommand cmd, object[] args)
		{
			if (args != null)
			{
				for (int i = 0; i < args.Length; i++)
				{
					object current = args[i];
					var param = new SqlParameter("@" + i, current);
					cmd.Parameters.Add(param.ParameterName, param.SqlDbType, param.Size).Value = current;
				}
			}

			cmd.Prepare();
		}

		// Traitement des paramètres et préparation de la commande - Par placeholder - ADO gère la reconnaissance des types

		/// <summary>
		/// Traitement des paramètres et préparation de la commande. 
		/// <br/>ADO gère la reconnaissance des types.
		/// <br/>Paramètres nommés par placeholder.
		/// </summary>
		/// <param name="cmd">La commande SQL à préparer.</param>
		/// <param name="args">Tableau des valeurs (MyRequestPlaceholder) pour les paramètres nommés.</param>
		private void ParametersPrepareByPlaceholder(SqlCommand cmd, MyRequestPlaceholder[] args)
		{
			if (args != null)
			{
				for (int i = 0; i < args.Length; i++)
				{
					var current = args[i];
					var param = new SqlParameter("@" + current.m_mask, current.m_value);
					cmd.Parameters.Add(param.ParameterName, param.SqlDbType, param.Size).Value = current.m_value;
				}
			}

			cmd.Prepare();
		}

		#endregion



		#region	Methods : requests

		/// <summary>
		/// Fonction de requête appelant ExecuteReader().
		/// <br/>Paramètres nommés par position.
		/// </summary>
		/// <param name="request">Chaîne de requête.</param>
		/// <param name="action">Delegate de traitement.</param>
		/// <param name="args">Tableau des valeurs (Object) pour les paramètres nommés.</param>
		public void RequestReader(string request, Action<SqlDataReader> action, object[] args = null)
		{
			using (_sqlCommand = new SqlCommand(request, _connection))
			{
				ParametersPrepareByPosition(_sqlCommand, args);

				// Exécuter la requête avec lecture de contenu 
				using (var reader = _sqlCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						action(reader);
					}
				}
			}
		}

		/// <summary>
		/// Fonction de requête appelant ExecuteReader().
		/// <br/>Paramètres nommés par placeholder.
		/// </summary>
		/// <param name="request">Chaîne de requête.</param>
		/// <param name="action">Delegate de traitement.</param>
		/// <param name="args">Tableau des valeurs (MyRequestPlaceholder) pour les paramètres nommés.</param>
		public void RequestReader(string request, Action<SqlDataReader> action, MyRequestPlaceholder[] args)
		{
			using (_sqlCommand = new SqlCommand(request, _connection))
			{
				ParametersPrepareByPlaceholder(_sqlCommand, args);

				// Exécuter la requête avec lecture de contenu 
				using (var reader = _sqlCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						action(reader);
					}
				}
			}
		}

		/// <summary>
		/// Fonction de requête appelant ExecuteNonQuery(). 
		/// <br/>Retourne le nombre de lignes concernées par la requête.
		/// <br/>Paramètres nommés par position.
		/// </summary>
		/// <param name="request">Chaîne de requête.</param>
		/// <param name="args">Tableau des valeurs (Object) pour les paramètres nommés.</param>
		/// <returns>(int) Nombre de lignes concernées par la requête.</returns>
		public int RequestNonQuery(string request, object[] args = null)
		{
			using (_sqlCommand = new SqlCommand(request, _connection))
			{
				ParametersPrepareByPosition(_sqlCommand, args);

				// Exécuter la requête avec retour de nombre de lignes concernées (0 si erreur ou pas d'action).
				return _sqlCommand.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Fonction de requête appelant ExecuteNonQuery(). 
		/// <br/>Retourne le nombre de lignes concernées par la requête.
		/// <br/>Paramètres nommés par placeholder.
		/// </summary>
		/// <param name="request">Chaîne de requête.</param>
		/// <param name="args">Tableau des valeurs (MyRequestPlaceholder) pour les paramètres nommés.</param>
		/// <returns>(int) Nombre de lignes concernées par la requête.</returns>
		public int RequestNonQuery(string request, MyRequestPlaceholder[] args)
		{
			using (_sqlCommand = new SqlCommand(request, _connection))
			{
				ParametersPrepareByPlaceholder(_sqlCommand, args);

				// Exécuter la requête avec retour de nombre de lignes concernées (0 si erreur ou pas d'action)
				return _sqlCommand.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Fonction de requête appelant ExecuteScalar(). 
		/// <br/>Retourne un objet (nullable).
		/// <br/>Paramètres nommés par position.
		/// </summary>
		/// <param name="request">Chaîne de requête.</param>
		/// <param name="args">Tableau des valeurs (Object) pour les paramètres nommés.</param>
		/// <returns>(object) Un ou plusieurs champs.</returns>
		public object RequestScalar(string request, object[] args = null)
		{
			using (_sqlCommand = new SqlCommand(request, _connection))
			{
				ParametersPrepareByPosition(_sqlCommand, args);

				// Exécuter la requête avec retour d'un objet (ou null) représentant une valeur
				return _sqlCommand.ExecuteScalar();
			}
		}

		/// <summary>
		/// Fonction de requête appelant ExecuteScalar(). 
		/// <br/>Retourne un objet (nullable).
		/// <br/>Paramètres nommés par placeholder.
		/// </summary>
		/// <param name="request">Chaîne de requête.</param>
		/// <param name="args">Tableau des valeurs (MyRequestPlaceholder) pour les paramètres nommés.</param>
		/// <returns>(object) Un ou plusieurs champs.</returns>
		public object RequestScalar(string request, MyRequestPlaceholder[] args)
		{
			using (_sqlCommand = new SqlCommand(request, _connection))
			{
				ParametersPrepareByPlaceholder(_sqlCommand, args);

				// Exécuter la requête avec retour d'un objet (ou null) représentant une valeur
				return _sqlCommand.ExecuteScalar();
			}
		}

		#endregion
	}
}
