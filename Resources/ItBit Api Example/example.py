from itbit_api import itBitApiConnection
import json
import hashlib
import binascii

client_key = '<You client key here>'
secret_key = '<Your key here>'
userId = '<Your user id here>'
walletId = '<Your wallet id here>'

#create the api connection
itbit_api_conn = itBitApiConnection(clientKey=client_key, secret=secret_key, userId=userId)

#get all the wallets for the user
wallets = itbit_api_conn.get_all_wallets().json()

#get the id of the wallet named Wallet
walletId = next(wallet for wallet in wallets if wallet['name'] == 'Wallet')['id']

#print the trades for the selected wallet
#print(json.dumps(itbit_api_conn.get_wallet_trades(walletId).json(), indent=2))

#create a new order
itbit_api_conn.create_order(walletId,  "buy", "XBT", "1.00", "28.00", "XBTUSD")


sha256_hash = hashlib.sha256()
hash_digest = sha256_hash.digest()
#print ("hash_digst: " + str(binascii.hexlify(hash_digest)))
sha256_hash.update('2["POST","https://beta-api.itbit.com/v1/wallets/118882b3-8102-427d-9546-b3baacc3ded1/orders/","{\"side\": \"buy\", \"type\": \"limit\", \"currency\": \"XBT\", \"price\": \"28.00\", \"instrument\": \"XBTUSD\", \"amount\": \"1.00\"}","2","1435956563806"]'.encode('utf8'))
hash_digest = sha256_hash.digest()
#print ("hash_digst: " + str(binascii.hexlify(hash_digest)))


