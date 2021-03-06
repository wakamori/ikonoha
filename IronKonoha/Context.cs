﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IronKonoha
{
	public enum ReportLevel
	{
		CRIT,
		ERR,
		WARN,
		INFO,
		PRINT,
		DEBUG,
	}

	public class KKeyWord : Symbol
	{
		public KeywordType Type { get; set; }
	}

	public class KPackage
	{

	}

	// struct KDEFINE_SYNTAX
	public class KDEFINE_SYNTAX
	{
		public string name { get; set; }
		public KeywordType kw { get; set; }
		public SynFlag flag { get; set; }
		public string rule { get; set; }
		public string op2 { get; set; }
		public string op1 { get; set; }
		public int priority_op2 { get; set; }
		public KonohaType type { get; set; }
		public StmtParser ParseStmt { get; set; }
		public ExprParser ParseExpr { get; set; }
		public StmtTyChecker TopStmtTyCheck { get; set; }
		public StmtTyChecker StmtTyCheck { get; set; }
		public StmtTyChecker ExprTyCheck { get; set; }
	}

	[Flags]
	public enum SPOL
	{
		TEXT = (1 << 0),
		ASCII = (1 << 1),
		UTF8 = (1 << 2),
		POOL = (1 << 3),
		NOCOPY = (1 << 4),
	}

	// kmodsugar_t;
	public class KModSugar : KModShare
	{
		public KonohaClass cToken { get; set; }
		public KonohaClass cExpr { get; set; }
		public KonohaClass cStmt { get; set; }
		public KonohaClass cBlock { get; set; }
		public KonohaClass cKonohaSpace { get; set; }
		public KonohaClass cGamma { get; set; }
		public KonohaClass cTokenArray { get; set; }
		public ICollection<string> keywordList { get{ return keywordMap.Keys; } }
		private IDictionary<string, KKeyWord> keywordMap;
		public IList<string> packageList { get; set; }
		public IDictionary<string, KPackage> packageMap;
		//public ExprParser UndefinedParseExpr { get; set; }
		public StmtTyChecker UndefinedStmtTyCheck { get; set; }
		public StmtTyChecker UndefinedExprTyCheck { get; set; }
		//public ExprParser ParseExpr_Term { get; set; }
		//public ExprParser ParseExrp_Op { get; set; }
		public Func<Context, string, uint, Symbol, KKeyWord> keyword { get; set; }
		private Action<Context, KonohaSpace, int, Tokenizer.FTokenizer, KMethod> KonohaSpace_setTokenizer { get; set; }
		public Func<Context, KonohaExpr, KonohaType, KObject, KonohaExpr> Expr_setConstValue { get; set; }
		public Func<Context, KonohaExpr, KonohaType, uint, KonohaExpr> Expr_setNConstValue { get; set; }
		public Func<Context, KonohaExpr, KonohaExpr, KonohaType, int, KGamma, KonohaExpr> Expr_setVariable { get; set; }
		public Func<Context, KStatement, KKeyWord, Token, Token> Stmt_token { get; set; }
		public Func<Context, KStatement, KKeyWord, KonohaExpr, KonohaExpr> Stmt_expr { get; set; }
		public Func<Context, KStatement, KKeyWord, string, string> Stmt_text { get; set; }
		public Func<Context, KStatement, KKeyWord, BlockExpr, BlockExpr> Stmt_block { get; set; }
		public Func<Context, KonohaExpr, uint, KGamma, KonohaType, int, KonohaExpr> Expr_tyCheckAt { get; set; }
		public Func<Context, KStatement, Symbol, KGamma, KonohaType, int, bool> Stmt_tyCheckAt { get; set; }
		public Func<Context, BlockExpr, KGamma, bool> Block_tyCheckAt { get; set; }
		public Func<Context, KonohaExpr, KMethod, KGamma, KonohaType, KonohaExpr> Expr_tyCheckCallParams { get; set; }
		public Func<Context, KonohaType, KMethod, KGamma, int, object[], KonohaExpr> new_TypedMethodCall { get; set; }
		public Action<Context, KStatement, KMethod, int, object[]> Stmt_toExprCall { get; set; }
		public Func<Context, int, LineInfo, int, string, object[], uint> p { get; set; }
		public Func<Context, KonohaExpr, int, LineInfo> Expr_uline { get; set; }
		public Func<Context, KonohaSpace, Symbol, int, Syntax> KonohaSpace_syntax { get; set; }
		public Action<Context, KonohaSpace, Symbol, KDEFINE_SYNTAX> KonohaSpace_defineSyntax { get; set; }
		public Func<Context, IList<Token>, int, int, IList<object>, bool> makeSyntaxRule { get; set; }
		public Func<Context, KonohaSpace, KStatement, IList<Token>, int, int, int, BlockExpr> new_block { get; set; }
		public Action<Context, BlockExpr, KStatement, KStatement> Block_insertAfter { get; set; }
		//public Func<Context, KStatement, IList<Token>, int, int, KonohaExpr> Stmt_newExpr2 { get; set; }
		public Func<Context, Syntax, int, object[], KonohaExpr> new_ConsExpr { get; set; }
		public Func<Context, KStatement, KonohaExpr, IList<Token>, int, int, int, KonohaExpr> Stmt_addExprParams { get; set; }
		//public Func<Context, KonohaExpr, KStatement, IList<Token>, int, int, int, KonohaExpr> Expr_rightJoin { get; set; }

		public KModSugar()
		{
			keywordMap = new Dictionary<string, KKeyWord>();
			// temp
			/*
			keywordMap["=="] = new KKeyWord() { Type = KeywordType.EQ };
			keywordMap["$INT"] = new KKeyWord() { Type = KeywordType.TKInt };
			keywordMap["$expr"] = new KKeyWord() { Type = KeywordType.Expr };
			 * */
		}

		public void AddKeyword(string name, KeywordType kw)
		{
			keywordMap.Add(name, new KKeyWord() { Type = kw });
		}

		public KKeyWord keyword_(string name, Symbol def)
		{
			return kmap_getcode(name, SPOL.ASCII | SPOL.POOL, def);
		}

		// ast.h
		// static KMETHOD UndefinedParseExpr(CTX, ksfp_t *sfp _RIX)
		public static KonohaExpr UndefinedParseExpr(Context ctx, Syntax syn, KStatement stmt, IList<Token> tls, int start, int c, int end)
		{
			Token tk = tls[c];
			ctx.SUGAR_P(ReportLevel.ERR, tk.ULine, 0, "undefined expression parser for '{0}'", tk.Text);
			return null;
		}

		// static KMETHOD ParseExpr_Op(CTX, ksfp_t *sfp _RIX)
		public static KonohaExpr ParseExpr_Op(Context ctx, Syntax syn, KStatement stmt, IList<Token> tls, int s, int c, int e)
		{
			Token tk = tls[c];
			KonohaExpr expr = null;
			KonohaExpr rexpr = stmt.newExpr2(ctx, tls, c + 1, e);
			KMethod mn = (s == c) ? syn.Op1 : syn.Op2;
			if (mn != null && syn.ExprTyCheck == ctx.kmodsugar.UndefinedExprTyCheck)
			{
				//kToken_setmn(tk, mn, (s == c) ? MNTYPE_unary: MNTYPE_binary);
				syn = stmt.ks.GetSyntax(KeywordType.ExprMethodCall);  // switch type checker
			}
			if (s == c)
			{ // unary operator
				expr = new ConsExpr(ctx, syn, tk, rexpr);
			}
			else
			{   // binary operator
				KonohaExpr lexpr = stmt.newExpr2(ctx, tls, s, c);
				expr = new ConsExpr(ctx, syn, tk, lexpr, rexpr);
			}
			return expr;
		}

		// static KMETHOD ParseExpr_Term(CTX, ksfp_t *sfp _RIX)
		public static KonohaExpr ParseExpr_Term(Context ctx, Syntax syn, KStatement stmt, IList<Token> tls, int s, int c, int e)
		{
			Debug.Assert(s == c);
			Token tk = tls[c];
			KonohaExpr expr = new TermExpr();
				//new_W(Expr, SYN_(kStmt_ks(stmt), tk->kw));
			//Expr_setTerm(expr, 1);
			expr.tk = tk;
			return Expr_rightJoin(ctx, expr, stmt, tls, s + 1, c + 1, e);
		}

		// static kExpr *Expr_rightJoin(CTX, kExpr *expr, kStmt *stmt, kArray *tls, int s, int c, int e)
		public static KonohaExpr Expr_rightJoin(Context ctx, KonohaExpr expr, KStatement stmt, IList<Token> tls, int s, int c, int e)
		{
			if(c < e && expr != null) {
				//WARN_Ignored(_ctx, tls, c, e);
			}
			return expr;
		}

		public KKeyWord kmap_getcode(string name, SPOL spol, Symbol def)
		{
			if (keywordMap.ContainsKey(name))
			{
				return keywordMap[name];
			}
			if (def == Symbol.NewID)
			{
				//keywordList.Add(name);
				//keywordMap.Add(name, null);
			}
			return null;
		}
	}

	public class KGamma : KObject
	{

	}

	public class Errors
	{
		public List<string> strings { get; set; }

		public uint Count { get; set; }

		public Errors()
		{
			strings = new List<string>();
		}
	}

	public class CTXSugar : KModLocal
	{
		public IList<Token> tokens { get; private set; }
		public IList<Token> cwb { get; private set; }
		public int err_count { get; set; }
		public Errors errors { get; private set; }
		public BlockExpr singleBlock { get; private set; }
		public KGamma gma { get; private set; }
		public IList<object> lvarlist { get; private set; }
		public IList<KMethod> definedMethods { get; private set; }

		public CTXSugar()
		{
			errors = new Errors();
		}
	}

	public class KMemShare
	{

	}

	public class KMemLocal
	{

	}

	// struct kmodshare_t;
	public class KModShare
	{

	}

	public class KModLocal
	{
		public KModSugar modsugar { get; set; }
	}

	public class KShare
	{
		public Dictionary<string, Symbol> SymbolMap { get; set; }
		public KShare()
		{
			SymbolMap = new Dictionary<string, Symbol>();
		}
	}

	public class KArray : KObject
	{

	}

	public class KLocal
	{

	}

	public class KStack
	{

	}

	public class KLogger
	{

	}

	public class KObject
	{
		public object magicflag { get; set; }

		public KonohaClass kclass { get; private set; }

		public KArray kvproto { get; set; }
	}

	public class KNumber : KObject
	{
		protected object value;

		public KNumber(int val)
		{
			value = val;
		}

		public KNumber(uint val)
		{
			value = val;
		}

		public KNumber(long val)
		{
			value = val;
		}

		public KNumber(ulong val)
		{
			value = val;
		}

		public KNumber(double val)
		{
			value = val;
		}

		public KNumber(short val)
		{
			value = val;
		}

		public int ToInt()
		{
			return (int)(value ?? 0);
		}

		public uint ToUInt()
		{
			return (uint)(value ?? 0);
		}

		public long ToLong()
		{
			return (long)(value ?? 0);
		}

		public ulong ToULong()
		{
			return (ulong)(value ?? 0);
		}

		public float ToFloat()
		{
			return (float)(value ?? 0);
		}

		public double ToDouble()
		{
			return (double)(value ?? 0);
		}

		public bool ToBoolean()
		{
			return (bool)(value ?? false);
		}

		public override string ToString()
		{
			return value == null ? "" : value.ToString();
		}
	}

	public class KBoolean : KNumber
	{
		public KBoolean(bool val)
			: base(val ? 1 : 0)
		{
		}
	}

	public enum ModID
	{
		Logger = 0,
		GC = 1,
		Code = 2,
		Sugar = 3,
		Float = 11,
		JIT = 12,
		IConv = 13,
		IO = 14,
		LLVM = 15,
		REGEX = 16,
	}

	// struct kcontext_t
	public class Context
	{
		public List<KMemShare> memshare { get; private set; }
		public List<KMemLocal> memlocal { get; private set; }
		public KShare share { get; private set; }
		public KStack stack { get; private set; }
		public KLogger logger { get; private set; }
		public List<KModShare> modshare { get; private set; }
		public List<KModLocal> modlocal { get; private set; }
		public uint KErrorNo { get; private set; }
		public CTXSugar ctxsugar { get { return modlocal[(int)ModID.Sugar] as CTXSugar; } }
		public KModSugar kmodsugar { get { return modshare[(int)ModID.Sugar] as KModSugar; } }

		public Context()
		{
			// とりあえず4つまでうめておく
			modshare = new List<KModShare>();
			modshare.Add(null);
			modshare.Add(null);
			modshare.Add(null);
			modshare.Add(new KModSugar());

			modlocal = new List<KModLocal>();
			modlocal.Add(null);
			modlocal.Add(null);
			modlocal.Add(null);
			modlocal.Add(new CTXSugar());

			share = new KShare();
		}

		public string GetErrorTypeString(ReportLevel pe)
		{
			switch (pe)
			{
				case ReportLevel.CRIT:
				case ReportLevel.ERR:
					return "(error)";
				case ReportLevel.WARN:
					return "(warning)";
				case ReportLevel.INFO:
					throw new NotImplementedException();
					/*if (CTX_isInteractive() || CTX_isCompileOnly() || verbose_sugar)
					{
						return "(info)";
					}*/
					return null;
				case ReportLevel.DEBUG:
					throw new NotImplementedException();
					/*if (verbose_sugar)
					{
						return "(debug)";
					}*/
					return null;
			}
			return "(unknown)";
		}

		// static size_t sugar_p(CTX, int pe, kline_t uline, int lpos, const char *fmt, ...)
		public uint SUGAR_P(ReportLevel pe, LineInfo line, int lpos, string format, params object[] param)
		{
			return vperrorf(pe, line, lpos, format, param);
		}

		// static size_t vperrorf(CTX, int pe, kline_t uline, int lpos, const char *fmt, va_list ap)
		uint vperrorf(ReportLevel pe, LineInfo uline, int lpos, string fmt, params object[] ap)
		{
			string msg = GetErrorTypeString(pe);
			uint errref = unchecked((uint)-1);
			if (msg != null)
			{
				var sugar = this.ctxsugar;
				if (uline != null)
				{
					string file = uline.Filename;
					if (file == string.Empty)
						file = "0";
					Console.Write("{0} ({1}:{2}) ", msg, file, uline.LineNumber);
				}
				else
				{
					Console.Write(msg + ' ');
				}
				Console.Write(fmt, ap);
				errref = (uint)sugar.errors.Count;
				sugar.errors.strings.Add(msg);
				if (pe == ReportLevel.ERR || pe == ReportLevel.CRIT)
				{
					sugar.err_count++;
				}
				ReportError(pe, msg);
			}
			return errref;
		}

		void ReportError(ReportLevel pe, string msg)
		{
#if __MonoCS__
			Console.WriteLine(" - " + msg);
#else
			var color = Console.ForegroundColor;
			switch (pe)
			{
				case ReportLevel.CRIT:
				case ReportLevel.ERR:
				case ReportLevel.WARN:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case ReportLevel.INFO:
				case ReportLevel.PRINT:
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				default:
					break;
			}
			Console.WriteLine(" - " + msg);
			Console.ForegroundColor = color;
#endif
		}
		public int sugarerr_count { get; set; }
	}
}
