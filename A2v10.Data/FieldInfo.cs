﻿// Copyright © 2012-2017 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace A2v10.Data
{
	public struct FieldInfo
	{
		public String PropertyName { get; }
		public String TypeName { get; }
		public FieldType FieldType { get; }
		public SpecType SpecType { get; }
		public Boolean IsComplexField { get; }
		public Boolean IsLazy { get; }
		public Boolean IsMain { get; set; }
		public String[] MapFields { get; }

		public FieldInfo(String name)
		{
			PropertyName = null;
			TypeName = null;
			FieldType = FieldType.Scalar;
			SpecType = SpecType.Unknown;
			IsLazy = false;
			IsMain = false;
			MapFields = null;
			var x = name.Split('!');
			if (x.Length > 0)
				PropertyName = x[0];
			CheckField(x);
			if (x.Length > 1)
			{
				TypeName = x[1];
				FieldType = FieldType.Object;
			}
			if (x.Length > 2)
			{
				FieldType = DataHelpers.TypeName2FieldType(x[2]);
				if (FieldType == FieldType.Scalar || FieldType == FieldType.Array || FieldType == FieldType.Json)
					SpecType = DataHelpers.TypeName2SpecType(x[2]);
				IsLazy = x[2].Contains("Lazy");
				IsMain = x[2].Contains("Main");
			}
			if (x.Length == 4)
			{
				FieldType = FieldType.MapObject;
				MapFields = x[3].Split(':');
			}
			IsComplexField = PropertyName.Contains('.');
			CheckReservedWords();
		}

		static HashSet<String> _reservedWords = new HashSet<String>()
			{
				"Parent",
				"Root",
				"ParentId",
				"CurrentyKey",
				"ParentRowNumber",
				"ParentKey",
				"ParentGUID"
			};

		void CheckReservedWords()
		{
			if (_reservedWords.Contains(PropertyName))
			{
				throw new DataLoaderException($"PropertyName '{PropertyName}' is a reserved word");
			}
		}

		public void CheckValid()
		{
			if (!String.IsNullOrEmpty(PropertyName))
			{
				if (FieldType == FieldType.Json)
					return;
				if (String.IsNullOrEmpty(TypeName))
					throw new DataLoaderException($"If a property name ('{PropertyName}') is specified, then type name is required");
			}
		}

		public FieldInfo(FieldInfo source, String name)
		{
			// for complex fields only
			PropertyName = name;
			TypeName = null;
			FieldType = FieldType.Scalar;
			SpecType = source.SpecType;
			IsComplexField = false;
			IsLazy = false;
			IsMain = false;
			MapFields = null;
		}


		public Boolean IsVisible { get { return !String.IsNullOrEmpty(PropertyName); } }

		public Boolean IsArray { get { return FieldType == FieldType.Array; } }
		public Boolean IsObject { get { return FieldType == FieldType.Object; } }
		public Boolean IsMap { get { return FieldType == FieldType.Map; } }
		public Boolean IsMapObject { get { return FieldType == FieldType.MapObject; } }
		public Boolean IsTree { get { return FieldType == FieldType.Tree; } }
		public Boolean IsGroup { get { return FieldType == FieldType.Group; } }

		public Boolean IsObjectLike { get { return IsArray || IsObject || IsTree || IsGroup || IsMap || IsMapObject; } }

		public Boolean IsRefId { get { return SpecType == SpecType.RefId; } }
		public Boolean IsParentId { get { return SpecType == SpecType.ParentId; } }
		public Boolean IsId { get { return SpecType == SpecType.Id; } }
		public Boolean IsKey { get { return SpecType == SpecType.Key; } }
		public Boolean IsRowCount { get { return SpecType == SpecType.RowCount; } }
		public Boolean IsItems { get { return SpecType == SpecType.Items; } }
		public Boolean IsGroupMarker { get { return SpecType == SpecType.GroupMarker; } }
		public Boolean IsJson { get { return SpecType == SpecType.Json; } }

		private static void CheckField(String[] parts)
		{
			if (parts.Length == 2)
			{
				var p1 = parts[1];
				if (SpecType.TryParse(p1, out SpecType st))
				{
					// A special type is specified, but there are only two parts in the field name
					throw new DataLoaderException($"Invalid field name '{String.Join("!", parts)}'");
				}
			}
		}

		public Object ConvertToSpecType(Object dataVal)
		{
			if (dataVal == null)
				return null;
			switch (SpecType)
			{
				case SpecType.UtcDate:
					return ConvertToUtcDate(dataVal);
			}
			return dataVal;
		}

		public DateTime ConvertToUtcDate(Object dataVal)
		{
			if (!(dataVal is DateTime dt))
				throw new DataLoaderException($"The field with the qualifier 'UtcDate' must be of type 'datetime'");
			return DateTime.SpecifyKind(dt.ToLocalTime(), DateTimeKind.Unspecified);
		}
	}
}
