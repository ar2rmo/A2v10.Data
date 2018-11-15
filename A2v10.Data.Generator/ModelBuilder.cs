﻿// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Text;

namespace A2v10.Data.Generator
{
	public class ModelBuilder
	{
		public StringBuilder StringBuilder => _stringBuilder;
		public Boolean MultiTenant { get; set; }

		StringBuilder _stringBuilder;

		public ModelBuilder()
		{
			_stringBuilder = new StringBuilder();
		}


		public void BuildTenantParam()
		{
			if (!MultiTenant) return;
			_stringBuilder.AppendLine("@TenantId int,");
		}

		public String TenantParamEQ => MultiTenant ? "@TenantId = @TenantId," : String.Empty;

		public void Clear()
		{
			_stringBuilder = new StringBuilder();
		}

		public override String ToString()
		{
			return _stringBuilder.ToString();
		}
	}
}