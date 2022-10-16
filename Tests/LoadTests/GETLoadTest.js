import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  insecureSkipTLSVerify: true,
  noConnectionReuse: false,
  stages: [
    { duration: '30s', target: 5 },
    { duration: '1m', target: 5 },
    { duration: '20s', target: 1 },
  ],
  thresholds:{
    http_req_duration: ['p(99)<250']
  }
};

export default function () {

  const url = 'https://localhost:56021/applications';
  let guid = generateGuid()
  const payload = JSON.stringify({
    applicationId: guid,
    creditApplication: {
        amount: 5000,
        creditPeriodInMonths: 24,
        customerPersonalData: {
        firstName: 'Jan',
        lastName: 'Nowak',
        pesel: '9999'
      },
      declaration: {
        averageNetMonthlyIncome: 3000
      }
    }
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post(url, payload, params);
  check(res, { 'status was 201': (r) => r.status == 200 });
  sleep(1);
}

function generateGuid() {
  return Math.random().toString(36).substring(2, 15) +
      Math.random().toString(36).substring(2, 15);
  }