import 'dart:async';
import 'dart:math';

import 'package:flutter/material.dart';
import 'package:viia_sample/models/account.dart';
import 'package:viia_sample/models/transaction.dart';
import 'package:viia_sample/services/viia_service.dart';
import 'package:viia_sample/widgets/account_details.dart';
import 'package:viia_sample/widgets/base_list_cell_card.dart';
import 'package:viia_sample/widgets/transaction_cell.dart';

class AccountDetailsScreenArguments {
  final Account account;

  AccountDetailsScreenArguments(this.account);
}

class AccountDetailsScreen extends StatefulWidget {
  @override
  State createState() => new AccountDetailsScreenState();
}

class AccountDetailsScreenState extends State<AccountDetailsScreen> {
  ViiaService _viiaService = new ViiaService();
  Account _account;
  List<Transaction> _transactions = List<Transaction>();
  String _pagingToken;

  bool _initialLoadExecuted = false;
  final int _pageSize = 10;
  int _present = 0;

  @override
  void initState() {
    super.initState();
  }

  updateStateWithArguments(AccountDetailsScreenArguments arguments) async {
    setState(() {
          _account = arguments.account;
        });
    
    // This method gets executed frequently, but we want to do initial transaction load once, so we need to track state
    if(!_initialLoadExecuted) {
      _initialLoadExecuted = true;
      _loadMore(arguments.account.id);
    }
  }

  void _loadMore(String accountId) {
     _viiaService.getTransactions(accountId, pagingToken: _pagingToken, pageSize: _pageSize).then((paginatedTransactions) {
      setState(() {
              _pagingToken = paginatedTransactions.pagingToken;
              _present += _pageSize;
              _transactions.addAll(paginatedTransactions.transactions);
            });
     })
    .catchError((error) {
      print(error);
    });
  }

  @override
  Widget build(BuildContext context) {
    AccountDetailsScreenArguments arguments = ModalRoute.of(context).settings.arguments;
    updateStateWithArguments(arguments);
    return Scaffold(
      appBar: AppBar(
        title: Text(_account.name),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(50.0),
          child: AccountDetailsWidget.build(_account),
        )
      ),
      body: ListView.builder(
            // If paging token is not set AND we transaction list is not empty means that all transactions are loaded and there are no more, so we shouldn't show 'load more' button
            itemCount: (_transactions.isNotEmpty && (_pagingToken == null || _pagingToken.isEmpty)) ? _transactions.length : _transactions.length + 1,
            itemBuilder: (context, index) {
              return (index ==_transactions.length) ?
              Container(
                color: Colors.greenAccent,
                child: FlatButton(
                  child: Text("Load More"),
                  onPressed: () {
                    _loadMore(_account.id);
                  },
                )
              )
              : BaseCellCard.build(
                TransactionCell.build(_transactions[index])
              );
            },
      )
    );
  }
}