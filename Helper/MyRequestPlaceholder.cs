using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOMyRequestSQL.Helper
{
	/// <summary>
	/// Créer un placeholder pour MyRequest.
	/// </summary>
	internal class MyRequestPlaceholder
	{ 
		#region Fields

		/// <summary>
		/// Paramètre nommé. Dans la requête, doit être précédé de "@".
		/// </summary>
		public string m_mask;

		/// <summary>
		/// Valeur du paramètre nommé.
		/// </summary>
		public object m_value;

		#endregion



		#region Constructors

		/// <summary>
		/// Constructeur. Renseigner le masque et la valeur correspondante.
		/// </summary>
		/// <param name="mask">Le paramètre nommé, sans "@" de la requête.</param>
		/// <param name="value">Valeur (de type Object).</param>
		public MyRequestPlaceholder(string mask, object value)
		{
			m_mask = mask;
			m_value = value;
		}

		#endregion
	}
}
