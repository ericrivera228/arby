<?php

	//Simulate the post object
	$_POST['api_key'] = 'lRjbkntJdB0IrEpP';
	$_POST['nonce'] = 1;
	$_POST['signature'] = '5984da0c18faa3c05bbadb79516581c3d95321b1b689846f4ae56be6304045ee';

	$encoded_post = json_encode($_POST,JSON_NUMERIC_CHECK);
	
//Pull values from the _post object
//	$api_key1 = (!empty($_POST['api_key'])) ? preg_replace("/[^0-9a-zA-Z]/","",$_POST['api_key']) : false;
$api_signature1 = "5984da0c18faa3c05bbadb79516581c3d95321b1b689846f4ae56be6304045ee";

	// check if API key/signature received
//	if ($api_key1 && (strlen($api_key1) != 16 || strlen($api_signature1) != 64)) {
//		print("invalid stuff!");
		//	}

//	else
		//	{
		//		print("valid stuff!");
		//	}

$decoded = json_decode($encoded_post,1);
	$decoded['api_key'] = "lRjbkntJdB0IrEpP";
	$decoded['nonce'] = intval($decoded['nonce']);
	unset($decoded['signature']);

    $hash = hash_hmac('sha256',json_encode($decoded,JSON_NUMERIC_CHECK),"Aa2rls1MJFT3UuNwz0WdeHVkCv5cS7yf");

	if ($api_signature1 == $hash) {
		print("success!");
	}
	else
		print("error");
?>