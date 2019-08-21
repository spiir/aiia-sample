import 'dart:convert';

import 'package:viia_sample/models/resource.dart';

class Transaction {
  final String accountId;
  final double balance;
  final String date;
  final String id;
  final String originalText;
  final String state;
  final String text;
  final double amount;
  final String currency;
  final String type;
  Transaction({this.accountId, this.balance, this.date, this.id, this.originalText, this.state, this.text, this.amount, this.currency, this.type});

  factory Transaction.fromJson(Map<String,dynamic> json) {
    return Transaction(
      accountId: json['accountId'],
      balance: json['balance']['value'],
      date: json['date'],
      id: json['id'],
      originalText: json['originalText'],
      state: json['state'],
      text: json['text'],
      amount: json['transactionAmount']['value'],
      currency: json['transactionAmount']['currency'] ?? json['balance']['currency'],
      type: json['type']
    );
  }

}

class PaginatedTransactions {
  final List<Transaction> transactions;
  final String pagingToken; 

  PaginatedTransactions(this.pagingToken, this.transactions);

  static Resource<PaginatedTransactions> get all {
    return Resource(
      parse: (response) {
        final result = json.decode(response.body); 
        Iterable list = result['transactions'];
        return PaginatedTransactions(result['pagingToken'], list.map((model) => Transaction.fromJson(model)).toList());
      }
    );
  }
}