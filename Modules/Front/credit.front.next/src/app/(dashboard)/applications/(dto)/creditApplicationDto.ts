export interface WithCreditParams {
    amount: number;
    creditPeriodInMonths: number;
}

export interface WithDeclerations {
    declaration: DeclarationDto;
}

export interface WithCustomerPersonal {
    customerPersonalData: CustomerPersonalDto;
}

export interface CustomerPersonalDto {
    firstName: string;
    lastName: string;
    documentId: string;
}

export interface DeclarationDto {
    averageNetMonthlyIncome: number;
}

export interface WithCreditApplicationState{
    state: ApplicationState;
}

export interface ApplicationState{
    level: "ApplicationRegistered" | "DecisionGenerated" | "ContractSigned" | "ApplicationClosed";
    date: Date,
    contractSigningDate: Date,
    decision: "NotExists" | "Positive" | "Negative"
}