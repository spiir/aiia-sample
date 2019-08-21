import 'package:flutter/material.dart';
import 'package:viia_sample/models/account.dart';
import 'package:viia_sample/widgets/account_cell.dart';
import 'package:viia_sample/widgets/base_list_cell_card.dart';
import '../services/viia_service.dart';
import 'account_details_screen.dart';

class AccountsScreen extends StatefulWidget {
  @override
  State createState() => new AccountsScreenState();
}

class AccountsScreenState extends State<AccountsScreen> {
  ViiaService _viiaService = new ViiaService();
  List<Account> _accounts = List<Account>();

  @override
  void initState() {
    super.initState();
    populateAccountsList();
  }

  void populateAccountsList() {
    _viiaService.getAccounts().then((accounts) {
      setState(() {
        _accounts = accounts;
      });
    });
  }

  void logout() async {
    await _viiaService.logout();
    await Navigator.pushReplacementNamed(context, "/");
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Accounts'),
        actions: [
          IconButton(
            icon: Icon(Icons.power_settings_new),
            onPressed: logout,
          )
        ]
      ),
      body: ListView.builder(
          itemCount: _accounts.length,
          itemBuilder: (context, index) {
            return BaseCellCard.build(AccountCell.build(_accounts[index], () {
              Navigator.pushNamed(context, '/account-details',
                  arguments: AccountDetailsScreenArguments(_accounts[index]));
            }));
          }),
    );
  }
}
