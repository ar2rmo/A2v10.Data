﻿// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Xml;

using A2v10.Data.Interfaces;

namespace A2v10.Data.Providers.Xml
{
	public class XmlReader : IExternalDataReader
	{
		private readonly DataFile _file;

		public XmlReader(DataFile file)
		{
			_file = file;
		}

		public IExternalDataFile Read(Stream stream)
		{
			Int32 level = 0;
			Record currentRow = null;
			using (var rdr = System.Xml.XmlReader.Create(stream))
			{
				while (rdr.Read()) {
					if (rdr.NodeType == XmlNodeType.Element)
					{
						level += 1;
						if (level == 2)
						{
							currentRow = ReadRow(rdr);
						}
						else if (level == 3)
						{
							if (currentRow != null)
							{
								var name = rdr.Name;
								do
								{
									rdr.Read(); // text insinde
								} while (rdr.NodeType != XmlNodeType.Text);
								var value = rdr.Value;
								ReadValue(currentRow, name, value);
							}
						}
					}
					else if (rdr.NodeType == XmlNodeType.EndElement)
						level--;
				}
			}
			return _file;
		}

		Record ReadRow(System.Xml.XmlReader rdr)
		{
			var record = _file.CreateRecord();
			for (Int32 i= 0; i < rdr.AttributeCount; i++)
			{
				rdr.MoveToAttribute(i);
				ReadValue(record, rdr.Name, rdr.Value);
			}
			return record;
		}

		void ReadValue(Record record, String name, String value)
		{
			Int32 ix = _file.GetOrCreateField(name);
			record.SetFieldValueString(ix, name, value);
		}
	}
}
