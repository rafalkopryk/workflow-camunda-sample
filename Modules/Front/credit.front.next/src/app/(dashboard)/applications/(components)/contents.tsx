"use client"

import { InputAdornment, TextField, Typography } from "@mui/material";
import Grid from '@mui/material/Grid2';
import { WithCreditApplicationState, WithCreditParams, WithCustomerPersonal, WithDeclerations } from "../(dto)/creditApplicationDto";
import { Dispatch } from "react";
import AttachMoney from '@mui/icons-material/AttachMoney';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import InfoIcon from '@mui/icons-material/Info';

export function CreditParamsContent({ creditApplication, applicationId, onChange }: { applicationId?: string, creditApplication: WithCreditParams, onChange?: Dispatch<(prevState: WithCreditParams) => WithCreditParams> }) {
    const disabled = onChange === undefined;

    return (
        <>
            <Grid size={{ xs: 12 }}>
                <Typography>Credit params</Typography>
            </Grid>
            {applicationId && (<Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    disabled={disabled}
                    label="ApplicationId"
                    name="applicationId"
                    value={applicationId}
                    fullWidth
                />
            </Grid>)}
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    disabled={disabled}
                    label="Amount"
                    name="amount"
                    value={creditApplication.amount}
                    fullWidth
                    onChange={!disabled ? (e) =>
                        onChange!((prevState) => {
                            return {
                                ...prevState,
                                amount: Number(e.target.value)
                            }
                        })
                        : undefined
                    }
                    slotProps={{
                        input: {
                            startAdornment: <InputAdornment position="start"><AttachMoney /></InputAdornment>,
                        },
                    }}
                />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    disabled={disabled}
                    label="Credit Period (Months)"
                    name="creditPeriodInMonths"
                    value={creditApplication.creditPeriodInMonths}
                    fullWidth
                    onChange={!disabled ? (e) =>
                        onChange!((prevState) => {
                            return {
                                ...prevState,
                                creditPeriodInMonths: Number(e.target.value)
                            }
                        })
                        : undefined
                    }
                />
            </Grid>
        </>
    );
}


export function CustomerDeclarationsContent({ creditApplication, onChange }: { creditApplication: WithDeclerations, onChange?: Dispatch<(prevState: WithDeclerations) => WithDeclerations> }) {
    return (
        <>
            <Grid size={{ xs: 12 }}>
                <Typography>Declaration incomes</Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    label="Average Net Monthly Income"
                    name="declaration.averageNetMonthlyIncome"
                    value={creditApplication.declaration.averageNetMonthlyIncome}
                    onChange={onChange ? (e) =>
                        onChange((prevState) => ({
                            ...prevState,
                            declaration: {
                                ...prevState.declaration,
                                averageNetMonthlyIncome: Number(e.target.value),
                            }
                        })) : undefined
                    }
                    fullWidth
                    slotProps={{
                        input: {
                            startAdornment: <InputAdornment position="start"><AttachMoney /></InputAdornment>,
                        },
                    }}
                />
            </Grid>
        </>
    );
}

export function CustomerPersonalDataContent({ creditApplication, onChange }: { creditApplication: WithCustomerPersonal, onChange?: Dispatch<(prevState: WithCustomerPersonal) => WithCustomerPersonal> }) {
    return (
        <>
            <Grid size={{ xs: 12 }}>
                <Typography>Personal data</Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    label="First Name"
                    name="customerPersonalData.firstName"
                    value={creditApplication.customerPersonalData.firstName}
                    onChange={onChange ? (e) =>
                        onChange((prevState) => ({
                            ...prevState,
                            customerPersonalData: {
                                ...prevState.customerPersonalData,
                                firstName: e.target.value,
                            },
                        })) : undefined
                    }
                    fullWidth
                />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    label="Last Name"
                    name="customerPersonalData.lastName"
                    value={creditApplication.customerPersonalData.lastName}
                    onChange={onChange ? (e) =>
                        onChange((prevState) => ({
                            ...prevState,
                            customerPersonalData: {
                                ...prevState.customerPersonalData,
                                lastName: e.target.value,
                            },
                        })) : undefined
                    }
                    fullWidth
                />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    label="PESEL"
                    name="customerPersonalData.pesel"
                    value={creditApplication.customerPersonalData.pesel}
                    onChange={onChange ? (e) =>
                        onChange((prevState) => ({
                            ...prevState,
                            customerPersonalData: {
                                ...prevState.customerPersonalData,
                                pesel: e.target.value,
                            },
                        })) : undefined
                    }
                    fullWidth
                />
            </Grid>
        </>
    );
}

export function CreditStatusContent({ creditApplication }: { creditApplication: WithCreditApplicationState }) {

    const decisionIcon = (() => {
        switch (creditApplication.state.decision) {
            case "Positive": return <CheckCircleIcon color="success" />
            case "Negative": return <ErrorIcon color="error" />
            default: return <InfoIcon color="info" />
        }
    })();

    return (
        <>
            <Grid size={{ xs: 12 }}>
                <Typography>Application status</Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    disabled
                    label="Status"
                    name="state.level"
                    value={(() => {
                        switch (creditApplication.state.level) {
                            case "ApplicationRegistered": return "Application registered";
                            case "DecisionGenerated": return "Decision generated";
                            case "ContractSigned": return "Contract signed";
                            case "ApplicationClosed": return "Application closed";
                        }
                    })()}
                    fullWidth
                />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                <TextField
                    disabled
                    label="Decision"
                    name="state.decision"
                    value={(() => {
                        switch (creditApplication.state.level) {
                            case "ApplicationRegistered": return "Decision generating, wait...";
                            case "DecisionGenerated":
                            case "ContractSigned":
                            case "ApplicationClosed": return creditApplication.state.decision;
                        }
                    })()}
                    slotProps={{
                        input: {
                            endAdornment: <InputAdornment position="start">{decisionIcon}</InputAdornment>,
                        },
                    }}
                    fullWidth
                />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                {creditApplication.state.level === 'ContractSigned'
                    && (<TextField
                        disabled
                        label="ContractSigningDate"
                        name="state.contractSigningDate"
                        value={creditApplication.state.contractSigningDate}
                        fullWidth
                    />)}
            </Grid>
        </>
    );
}