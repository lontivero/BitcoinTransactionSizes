using System;
using NBitcoin;
using NBitcoin.Protocol;

namespace bitcoin_output_size
{
	class Program
	{
		static void Main(string[] args)
		{
			var key = new Key();
			var pubKey = key.PubKey;
			var p2pk = PayToPubkeyTemplate.Instance.GenerateScriptPubKey(pubKey);
			var p2pkh = PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(pubKey);
			var p2wpkh = PayToWitPubKeyHashTemplate.Instance.GenerateScriptPubKey(pubKey);

			var amount = Money.Coins(1.87654321m);
			var p2pkOutputSize = new TxOut(amount, p2pk).GetSerializedSize();
			var p2pkhOutputSize = new TxOut(amount, p2pkh).GetSerializedSize();
			var p2wpkhOutputSize = new TxOut(amount, p2wpkh).GetSerializedSize();

			Console.WriteLine($"Output types:");
			Console.WriteLine($"------------------------------------------");
			Console.WriteLine($"* P2PK output size       = {p2pkOutputSize} bytes");
			Console.WriteLine($"* P2PKH output size      = {p2pkhOutputSize} bytes");
			Console.WriteLine($"* P2WPKh output size     = {p2wpkhOutputSize} bytes\n");

			var builder = new TransactionBuilder();
			var tx = builder
				.AddCoins(new Coin(uint256.One, 0U, Money.Coins(2m), p2wpkh))
				.AddKeys(key)
				.Send(new Key().PubKey.WitHash, Money.Coins(2m))
				.BuildTransaction(true);
			
			Console.WriteLine($"Min Transaction size:");
			Console.WriteLine($"------------------------------------------");
			Console.WriteLine($"* Min Tx w/o witness size= {tx.GetSerializedSize(TransactionOptions.None)} bytes\n");
			Console.WriteLine($"* Min Tx size            = {tx.GetSerializedSize()} bytes");
			Console.WriteLine($"* Min Tx virtual size    = {tx.GetVirtualSize()} bytes <--- this is important!!\n");

			var txSize =  ( 1 + tx.Inputs[0].GetSerializedSize() ) 
						+ ( 1 + tx.Outputs[0].GetSerializedSize())
						+ sizeof(uint) // tx.Version
						+ sizeof(uint); // tx.LockTime.Value

			var witnessSize = new VarString( tx.Inputs[0].WitScript.ToBytes()).GetSerializedSize();
			var dummySize = 1;
			var txWitSize = txSize + dummySize + witnessSize;

			Console.WriteLine($"Min Transaction size calculation:");
			Console.WriteLine($"------------------------------------------");
			Console.WriteLine($"* Min Tx input size      = {tx.Inputs[0].GetSerializedSize()} bytes");
			Console.WriteLine($"* Min Tx output size     = {tx.Outputs[0].GetSerializedSize()} bytes");

			Console.WriteLine($"size(tx) = sizeof(version)");
			Console.WriteLine($"         + sizeof(locktime)");
			Console.WriteLine($"         + (1 + sizeof(min-tx-input)");
			Console.WriteLine($"         + (1 + sizeof(min-tx-output)");
			Console.WriteLine($"         + hasWitness ? (1 + sizeof(witness) \n");
			Console.WriteLine($"------------------------------------------");

			Console.WriteLine($"* Min Tx w/o witness size  = { txSize } bytes");
			Console.WriteLine($"* Min Tx with witness size = { txWitSize } bytes");
			Console.WriteLine($"* Min Tx virtual size      = { (((3 * txSize) + txWitSize) + 3) /4 } bytes <--- this is important!!\n");


			builder = new TransactionBuilder();
			tx = builder
				.AddCoins(new Coin(uint256.One, 0U, Money.Coins(2m), p2pkh))
				.AddKeys(key)
				.Send(new Key().PubKey.Hash, Money.Coins(2m))
				.BuildTransaction(true);
			
			Console.WriteLine($"P2PKH->P2PKH Min Transaction size:");
			Console.WriteLine($"------------------------------------------");
			Console.WriteLine($"  Transaction is valid {builder.Verify(tx)}");
			Console.WriteLine($"* Min Tx w/o witness size= {tx.GetSerializedSize(TransactionOptions.None)} bytes");
			Console.WriteLine($"* Min Tx size            = {tx.GetSerializedSize()} bytes");
			Console.WriteLine($"* Min Tx virtual size    = {tx.GetVirtualSize()} bytes!!\n");


			builder = new TransactionBuilder();
			tx = builder
				.AddCoins(new Coin(uint256.One, 0U, Money.Coins(2m), p2pkh))
				.AddKeys(key)
				.Send(new Key().PubKey.WitHash, Money.Coins(2m))
				.BuildTransaction(true);
			
			Console.WriteLine($"P2PKH->P2WPKH Min Transaction size:");
			Console.WriteLine($"------------------------------------------");
			Console.WriteLine($"  Transaction is valid {builder.Verify(tx)}");
			Console.WriteLine($"* Min Tx w/o witness size= {tx.GetSerializedSize(TransactionOptions.None)} bytes");
			Console.WriteLine($"* Min Tx size            = {tx.GetSerializedSize()} bytes");
			Console.WriteLine($"* Min Tx virtual size    = {tx.GetVirtualSize()} bytes!!\n");

		}
	}
}
