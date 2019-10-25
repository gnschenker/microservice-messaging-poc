const { Pool } = require('pg');
const sleep = require('sleep-promise');

const pool = new Pool(
    {
        host: 'db',
        user: 'pguser',
        password: 'topsecret',
        database: 'sampledb'
    }
);

const providers = [
    "VISA", "Mastercard", "AMEX", "American Express"
];

const cc_numbers = [
    "1234 5678 9012 3450",
    "1234 5678 9012 3451",
    "1234 5678 9012 3452",
    "1234 5678 9012 3453",
    "1234 5678 9012 3454",
    "1234 5678 9012 3455",
    "1234 5678 9012 3456",
    "1234 5678 9012 3457",
    "1234 5678 9012 3458",
    "1234 5678 9012 3459"
];

function getRandomValues(){
    var provider = providers[getRandomInt(providers.length)];
    var cc_number = cc_numbers[getRandomInt(cc_numbers.length)];
    var status = "SUCCESS";
    if(getRandomInt(10)<2){
        status = "FAIL";
    }
    return [provider, cc_number, status];
}

function getRandomInt(max) {
    return Math.floor(Math.random() * Math.floor(max));
}

const text = "INSERT INTO cc_authentications(provider, cc_number, status) VALUES($1,$2,$3) RETURNING *";


(async () => {
    while(true){
        var values = getRandomValues();
        try{
            const res = await pool.query(text, values);
            console.log(res.rows[0]);
            await sleep(100);
        } catch(e){
            console.log(e.stack);
        }
    }
})().catch(err => {
    console.error(err);
});
