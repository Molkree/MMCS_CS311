﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SimpleLangLexer
{

    public class LexerException : System.Exception
    {
        public LexerException(string msg)
            : base(msg)
        {
        }

    }

    public enum Tok
    {
        EOF,
        ID,
        COMMA,
        INUM,
        COLON,
        SEMICOLON,
        ASSIGN,
        BEGIN,
        END,
        CYCLE,
        PLUS,
        MINUS,
        MULT,
        DIV_SIGN,
        DIV,
        MOD,
        AND,
        OR,
        NOT,
        ADD_ASSIGN,
        SUB_ASSIGN,
        MULT_ASSIGN,
        DIV_ASSIGN,
        GREATER,
        LESS,
        GREATER_EQUAL,
        LESS_EQUAL,
        EQUAL,
        NOT_EQUAL
    }

    public class Lexer
    {
        private int position;
        private char currentCh;                      // Текущий символ
        public int LexRow, LexCol;                  // Строка-столбец начала лексемы. Конец лексемы = LexCol+LexText.Length
        private int row, col;                        // текущие строка и столбец в файле
        private TextReader inputReader;
        private Dictionary<string, Tok> keywordsMap; // Словарь, сопоставляющий ключевым словам константы типа TLex. Инициализируется процедурой InitKeywords 
        public Tok LexKind;                         // Тип лексемы
        public string LexText;                      // Текст лексемы
        public int LexValue;                        // Целое значение, связанное с лексемой LexNum

        private string CurrentLineText;  // Накапливает символы текущей строки для сообщений об ошибках
        

        public Lexer(TextReader input)
        {
            CurrentLineText = "";
            inputReader = input;
            keywordsMap = new Dictionary<string, Tok>();
            InitKeywords();
            row = 1; col = 0;
            NextCh();       // Считать первый символ в ch
            NextLexem();    // Считать первую лексему, заполнив LexText, LexKind и, возможно, LexValue
        }

        public void Init() {

        }

        private void PassSpaces()
        {
            while (char.IsWhiteSpace(currentCh))
            {
                NextCh();
            }
        }

        private void InitKeywords()
        {
            keywordsMap["begin"] = Tok.BEGIN;
            keywordsMap["end"] = Tok.END;
            keywordsMap["cycle"] = Tok.CYCLE;
            keywordsMap["div"] = Tok.DIV;
            keywordsMap["mod"] = Tok.MOD;
            keywordsMap["and"] = Tok.AND;
            keywordsMap["or"] = Tok.OR;
            keywordsMap["not"] = Tok.NOT;
        }

        public string FinishCurrentLine()
        {
            return CurrentLineText + inputReader.ReadLine();
        }

        private void LexError(string message)
        {
            System.Text.StringBuilder errorDescription = new System.Text.StringBuilder();
            errorDescription.AppendFormat("Lexical error in line {0}:", row);
            errorDescription.Append("\n");
            errorDescription.Append(FinishCurrentLine());
            errorDescription.Append("\n");
            errorDescription.Append(new String(' ', col - 1) + '^');
            errorDescription.Append('\n');
            if (message != "")
            {
                errorDescription.Append(message);
            }
            throw new LexerException(errorDescription.ToString());
        }

        private void NextCh()
        {
            // В LexText накапливается предыдущий символ и считывается следующий символ
            LexText += currentCh;
            var nextChar = inputReader.Read();
            if (nextChar != -1)
            {
                currentCh = (char)nextChar;
                if (currentCh != '\n')
                {
                    col += 1;
                    CurrentLineText += currentCh;
                }
                else
                {
                    row += 1;
                    col = 0;
                    CurrentLineText = "";
                }
            }
            else
            {
                currentCh = (char)0; // если достигнут конец файла, то возвращается #0
            }
        }

        public void NextLexem()
        {
            PassSpaces();
            // R К этому моменту первый символ лексемы считан в ch
            LexText = "";
            LexRow = row;
            LexCol = col;
            // Тип лексемы определяется по ее первому символу
            // Для каждой лексемы строится синтаксическая диаграмма
            if (currentCh == ';')
            {
                NextCh();
                LexKind = Tok.SEMICOLON;
            }
            else if (currentCh == ',')
            {
                NextCh();
                LexKind = Tok.COMMA;
            }
            else if (currentCh == ':')
            {
                NextCh();
                LexKind = Tok.COLON;
                if (currentCh == '=')
                {
                    LexKind = Tok.ASSIGN;
                    NextCh();
                }
            }
            else if (currentCh == '+')
            {
                NextCh();
                LexKind = Tok.PLUS;
                if (currentCh == '=')
                {
                    LexKind = Tok.ADD_ASSIGN;
                    NextCh();
                }
            }
            else if (currentCh == '-')
            {
                NextCh();
                LexKind = Tok.MINUS;
                if (currentCh == '=')
                {
                    LexKind = Tok.SUB_ASSIGN;
                    NextCh();
                }
            }
            else if (currentCh == '*')
            {
                NextCh();
                LexKind = Tok.MULT;
                if (currentCh == '=')
                {
                    LexKind = Tok.MULT_ASSIGN;
                    NextCh();
                }
            }
            else if (currentCh == '/')
            {
                NextCh();
                LexKind = Tok.DIV_SIGN;
                if (currentCh == '=')
                {
                    LexKind = Tok.DIV_ASSIGN;
                    NextCh();
                }
                else if (currentCh == '/')
                {
                    while (currentCh != '\n')
                    {
                        if (currentCh == (char)0) // in case we have comment in last line
                            break;
                        NextCh();
                    }
                    NextLexem();
                }
            }
            else if (currentCh == '>')
            {
                NextCh();
                LexKind = Tok.GREATER;
                if (currentCh == '=')
                {
                    LexKind = Tok.GREATER_EQUAL;
                    NextCh();
                }
            }
            else if (currentCh == '<')
            {
                NextCh();
                LexKind = Tok.LESS;
                if (currentCh == '=')
                {
                    LexKind = Tok.LESS_EQUAL;
                    NextCh();
                }
                else if (currentCh == '>')
                {
                    LexKind = Tok.NOT_EQUAL;
                    NextCh();
                }
            }
            else if (currentCh == '=')
            {
                NextCh();
                LexKind = Tok.EQUAL;
            }
            else if (currentCh == '{')
            {
                NextCh();
                while (currentCh != (char)0)
                {
                    if (currentCh == '}')
                        break;
                    NextCh();
                }
                if (currentCh == (char)0)
                    LexError("End-of-file found, '}' expected");
                NextCh();
                NextLexem();
            }
            else if (char.IsLetter(currentCh))
            {
                while (char.IsLetterOrDigit(currentCh))
                {
                    NextCh();
                }
                if (keywordsMap.ContainsKey(LexText))
                {
                    LexKind = keywordsMap[LexText];
                }
                else
                {
                    LexKind = Tok.ID;
                }
            }
            else if (char.IsDigit(currentCh))
            {
                while (char.IsDigit(currentCh))
                {
                    NextCh();
                }
                LexValue = Int32.Parse(LexText);
                LexKind = Tok.INUM;
            }
            else if ((int)currentCh == 0)
            {
                LexKind = Tok.EOF;
            }
            else
            {
                LexError("Incorrect symbol " + currentCh);
            }
        }

        public virtual void ParseToConsole()
        {
            do
            {
                Console.WriteLine(TokToString(LexKind));
                NextLexem();
            } while (LexKind != Tok.EOF);
        }

        public string TokToString(Tok t)
        {
            var result = t.ToString();
            switch (t)
            {
                case Tok.ID: result += ' ' + LexText;
                    break;
                case Tok.INUM: result += ' ' + LexValue.ToString();
                    break;
            }
            return result;
        }
    }
}
