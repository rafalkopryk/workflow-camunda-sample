"use client"

import { Dispatch, SetStateAction, useState } from 'react';
import { Card, CardContent } from '@mui/material';

import Grid from '@mui/material/Grid2';

import { useRouter } from 'next/navigation';
import { v7 as uuidv7 } from "uuid";
import { WithCreditParams, WithDeclerations, WithCustomerPersonal } from "@/app/(dashboard)/applications/(dto)/creditApplicationDto";
import { RegisterApplicationButton } from './(components)/buttons';
import { CreditParamsContent, CustomerDeclarationsContent, CustomerPersonalDataContent } from './(components)/contents';

type CreditApplication = WithCreditParams & WithDeclerations & WithCustomerPersonal;

type RegisterCreditApplicationDto = {
    applicationId: string;
    creditApplication: CreditApplication;
};

export default function ApplicationPage() {
    const [creditApplication, setCreditApplication] = useState<CreditApplication>({
        amount: 0,
        creditPeriodInMonths: 0,
        customerPersonalData: {
            firstName: '',
            lastName: '',
            pesel: '',
        },
        declaration: {
            averageNetMonthlyIncome: 0,
        }
    });

    const router = useRouter();

    return (
        <Card>
            <CardContent>
                <Grid container spacing={2}>

                    <CreditParamsContent
                        creditApplication={creditApplication}
                        onChange={setCreditApplication as Dispatch<SetStateAction<WithCreditParams>>} />

                    <CustomerDeclarationsContent
                        creditApplication={creditApplication}
                        onChange={setCreditApplication as Dispatch<SetStateAction<WithDeclerations>>} />

                    <CustomerPersonalDataContent
                        creditApplication={creditApplication}
                        onChange={setCreditApplication as Dispatch<SetStateAction<WithCustomerPersonal>>} />

                    <RegisterApplicationButton onRegisterApplication={register} />
                </Grid>
            </CardContent>
        </Card>
    );

    async function register() {
        const command: RegisterCreditApplicationDto = {
            applicationId: uuidv7(),
            creditApplication: creditApplication
        };

        const response = await fetch(`${process.env.APPLICATION_URL}/applications`, {
            body: JSON.stringify(command),
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
        });

        if (response.status !== 201) {
            throw new Error(`Failed to register application. Status: ${response.status}`);
        } else {
            router.push(`/applications/${command.applicationId}`);
        }
    }
}