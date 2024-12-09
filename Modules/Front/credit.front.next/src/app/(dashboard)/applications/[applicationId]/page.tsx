"use client"

import { Card, CardContent } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { WithCreditParams, WithCreditApplicationState } from '../(dto)/creditApplicationDto';
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { delay } from '../../../../../libs/delay';
import { CancelApplicationButton, SignContractButton } from '../(components)/buttons';
import { CreditParamsContent, CreditStatusContent } from '../(components)/contents';

type CreditApplicationDto = WithCreditParams & WithCreditApplicationState;

export default function ApplicationDetailsPage() {
    const [creditApplication, setCreditApplication] = useState<CreditApplicationDto>();
    const { applicationId } = useParams();

    useEffect(() => {
        runLongPooling();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    if (!creditApplication) {
        return <p>Loading...</p>
    }

    return (
        <Card>
            <CardContent>
                <Grid container spacing={2}>
                    <CreditParamsContent creditApplication={creditApplication} applicationId={applicationId as string} />
                    <CreditStatusContent creditApplication={creditApplication} />

                    <SignContractButton creditApplication={creditApplication} onSignContract={signConract} />
                    <CancelApplicationButton creditApplication={creditApplication} onCancelApplication={cancelApplication} />
                </Grid>
            </CardContent>
        </Card>
    );

    async function runLongPooling(){
        let timerPeriod: number = 2000;
        while(true){
            const data = await getApplication(applicationId as string);
            setCreditApplication(data);

            if (data?.state.level !== "ApplicationRegistered" && data?.state.level !== "DecisionGenerated") {
                break;
            }

            if (data?.state.level === "DecisionGenerated") {
                timerPeriod = 5000;
            }

            await delay(timerPeriod);
        }
    }
    
    async function getApplication(applicationId: string): Promise<CreditApplicationDto> {
        const response = await fetch(`${process.env.APPLICATION_URL}/applications/${applicationId}/`);
        if (response.status !== 200) {
            //todo
        }

        const data = await response.json();
        return data.creditApplication as CreditApplicationDto;
    }

    async function signConract() : Promise<void> {
        const id = applicationId as string;
        const response = await fetch(`${process.env.APPLICATION_URL}/applications/${id}/signature`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
        });

        if (response.status !== 200) {
            //todo
        }

        const data = await getApplication(id);
        setCreditApplication(data);
    }

    async function cancelApplication() {
        const id = applicationId as string;
        const response = await fetch(`${process.env.APPLICATION_URL}/applications/${id}/cancellation`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
        });

        if (response.status !== 200) {
            //todo
        }

        const data = await getApplication(id);
        setCreditApplication(data);
    }
}
