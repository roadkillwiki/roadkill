using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// Parses a HTML document for Hx (e.g. H1,H2) elements and produces a table of contents.
	/// </summary>
	public class TocParser
	{
		// Needs object:
		// Header
		// - Level (int)
		// - Children (List<Header>)
		// - Title
		// - Id

		// #1 Get a list of H1s
		//   Create a new header object, with its title and id.
		//   Get the inner HTML for the h1
		//   Parse the HTML looking for H2s
		//     Do #1
		//     Recurse
		//   Insert <a> tag inside the inner html.
	}
}
