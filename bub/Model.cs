using System.Collections.Generic;
using System;

namespace bub.Model
{
    public abstract class Cell
    {

    }

    public abstract class Turn
    {

    }

    public abstract class Selection<C>
        where C : Cell
    {

    }

    public abstract class Field<C, S>
        where C: Cell
        where S: Selection<C>
    {
        private Array m_Field;

        public int SizeX
        {
            get;
            protected set;
        }

        public int SizeY
        {
            get;
            protected set;
        }

        public int Score
        {
            get;
            protected set;
        }

        public Field(Field<C, S> copy)
        {
            SizeX = copy.SizeX;
            SizeY = copy.SizeY;

            Array.Copy(copy.m_Field, m_Field, copy.m_Field.Length);
        }

        public Field(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;

            m_Field = Array.CreateInstance(typeof(C), SizeY, SizeX);
        }

        public abstract void Generate();
    }

    public abstract class Game<F, S, C, T>
        where C : Cell
        where S : Selection<C>
        where F : Field<C, S>
        where T : Turn
    {
        private List<F> m_Fields;

        public Game()
        {

        }

        public abstract void NewGame();
        public abstract bool IsGameOver { get; }
        public abstract bool MakeTurn(T turn);
        public abstract bool Undo();
        public abstract bool CanUndo { get; }
    }
}