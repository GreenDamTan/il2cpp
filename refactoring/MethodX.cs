﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace il2cpp
{
	// 指令
	internal class InstInfo
	{
		public OpCode OpCode;
		public object Operand;
		public int Offset;

		public bool IsBrTarget;
		public bool IsProcessed;
	}

	internal class MethodX : GenericArgs
	{
		// 所属类型
		public readonly TypeX DeclType;

		// 方法全名
		public readonly string DefFullName;
		// 方法名
		public readonly string DefName;
		// 方法签名
		public readonly MethodSig DefSig;
		// this 签名
		public readonly TypeSig DefThisSig;
		// 方法属性
		public readonly MethodAttributes DefAttr;
		// 方法异常处理器
		public IList<ExceptionHandler> DefHandlers;
		// 方法指令列表
		public IList<Instruction> DefInstList;

		// 唯一名称
		private string NameKey;

		// 返回值类型
		public TypeSig ReturnType;
		// 参数类型列表, 包含 this 类型
		public IList<TypeSig> ParamTypes;
		public IList<TypeSig> ParamAfterSentinel;
		// 局部变量类型列表
		public IList<TypeSig> LocalTypes;
		//! 异常处理器列表
		// 指令列表
		public InstInfo[] InstList;

		public bool HasThis => DefSig.HasThis;

		internal MethodX(TypeX declType, MethodDef metDef)
		{
			DeclType = declType;
			DefFullName = metDef.FullName;
			DefName = metDef.Name;
			DefSig = metDef.MethodSig;
			DefAttr = metDef.Attributes;

			if (HasThis)
			{
				Debug.Assert(!metDef.IsStatic);
				Debug.Assert(metDef.Parameters[0].IsHiddenThisParameter);
				DefThisSig = metDef.Parameters[0].Type;
			}

			if (metDef.HasBody)
			{
				if (metDef.Body.HasExceptionHandlers)
					DefHandlers = metDef.Body.ExceptionHandlers;

				if (metDef.Body.HasVariables)
				{
					LocalTypes = new List<TypeSig>();
					foreach (var loc in metDef.Body.Variables)
					{
						Debug.Assert(loc.Index == LocalTypes.Count);
						LocalTypes.Add(loc.Type);
					}
				}

				if (metDef.Body.HasInstructions)
					DefInstList = metDef.Body.Instructions;
			}
		}

		public string GetNameKey()
		{
			if (NameKey == null)
			{
				Debug.Assert(ReturnType != null);
				Debug.Assert(ParamTypes != null);

				// Name|RetType<GenArgs>(ArgList)|CC|Attr
				StringBuilder sb = new StringBuilder();
				NameManager.MethodSigName(sb, DefName, ReturnType, GenArgs, ParamTypes, DefSig.CallingConvention);
				sb.Append('|');
				sb.Append(((uint)DefAttr).ToString("X"));

				NameKey = sb.ToString();
			}
			return NameKey;
		}
	}
}