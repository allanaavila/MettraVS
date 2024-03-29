﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyntaxTreeManualTraversal.Titular; 

namespace SyntaxTreeManualTraversal.Contas
{
    internal class ContaCorrente
    {
        public static int TotalDeContasCriadas { get; private set; }


        private int numero_agencia;
        public int Numero_agencia
        {
            get { return this.numero_agencia; }
            private set
            {
                if (value > 0)
                {
                    this.numero_agencia = value;
                }
            }
        }
        public string Conta { get; set; }
        private double saldo = 100;
        public Cliente Titular { get; set; }


        //metodos
        public void Depositar(double valor)
        {
            saldo += valor;
        }

        public bool Sacar(double valor)
        {
            if (valor <= saldo)
            {
                saldo -= valor;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Transferir(double valor, ContaCorrente destino)
        {
            if (saldo < valor)
            {
                return false;
            }
            if (valor < 0)
            {
                return false;
            }
            else
            {
                saldo = saldo - valor;
                destino.saldo += valor;
                return true;
            }
        }

        public void SetSaldo(double valor)
        {
            if (valor < 0)
            {
                return;
            }
            else
            {
                this.saldo = valor;
            }
        }

        public double GetSaldo()
        {
            return this.saldo;
        }

        public ContaCorrente(int numero_agencia, string numero_conta)
        {
            this.numero_agencia = numero_agencia;
            this.Conta = numero_conta;

            TotalDeContasCriadas++;
        }

        public ContaCorrente(Cliente titular, int numero_agencia, string conta)
        {
            Titular = titular;
            Numero_agencia = numero_agencia;
            Conta = conta;
        }
    }
}
